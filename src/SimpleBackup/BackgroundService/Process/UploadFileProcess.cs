using System.Diagnostics;
using System.Security.Cryptography;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Compression.Interfaces;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption;
using Cloud_ShareSync.Core.Cryptography.FileEncryption.Types;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;
using Cloud_ShareSync.Core.SharedServices;
using Cloud_ShareSync.SimpleBackup.Interfaces;
using Cloud_ShareSync.SimpleBackup.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleBackup.Process {
    internal class UploadFileProcess : IUploadFileProcess {

        #region Fields

        private readonly ActivitySource _source = new( "UploadFileProcess" );
        private readonly ILogger<UploadFileProcess> _log;
        private readonly ICompression? _compress = null;
        private readonly Hashing _fileHash;
        private readonly ManagedChaCha20Poly1305? _crypto;
        private readonly BackBlazeB2 _backBlaze;
        private readonly CloudShareSyncServices _services;
        private readonly BackupConfig _backupConfig;
        private readonly B2Config _backblazeConfig;
        private readonly DatabaseConfig _databaseConfig;

        private readonly SemaphoreSlim _semaphore = new( 0, 1 );

        #endregion Fields

        public UploadFileProcess(
            BackupConfig backupConfig,
            B2Config backblazeConfig,
            DatabaseConfig databaseConfig,
            CompressionConfig? compressionConfig,
            ILogger<UploadFileProcess> log
        ) {
            _backupConfig = backupConfig;
            _backblazeConfig = backblazeConfig;
            _databaseConfig = databaseConfig;
            _log = log;
            if (_backupConfig.CompressBeforeUpload == true && compressionConfig != null) {
                _compress = new CompressionIntermediary( compressionConfig, _log );
            }
            _fileHash = new( _log );
            _services = new CloudShareSyncServices( _databaseConfig.SqliteDBPath, _log );
            _semaphore.Release( 1 );
            _backBlaze = new( _backblazeConfig, _log );
            _crypto = (backupConfig.EncryptBeforeUpload) ? new( _log ) : null;
        }


        /// <summary>
        /// The high level/abstracted upload file process for a given UploadFileInput.
        /// </summary>
        /// <param name="ufInput"></param>
        public async Task Process( UploadFileInput ufInput ) {
            using Activity? activity = _source.StartActivity( "Process" )?.Start( );

            FileInfo uploadFile = ufInput.UploadFile;
            string uploadPath = ufInput.RelativePath;
            PrimaryTable tabledata = ufInput.TableData;
            _log.LogInformation( "PrimaryTabledata:\n{string}", tabledata );

            uploadFile = CopyFileToWorkingDir( uploadFile );

            await SetOriginalFileHash( uploadFile, tabledata );

            uploadFile = await EncryptFile( uploadFile, tabledata );

            uploadFile = CompressFile( uploadFile, tabledata );

            string sha512filehash = await SetUploadFileHash( uploadFile, tabledata );

            // Upload File.
            await UploadFileToB2(
                tabledata,
                uploadFile,
                uploadFile.FullName,
                uploadPath,
                tabledata.FileHash // Use Original FileHash in b2 metadata.
            );

            // Remove file from working directory (if needed).
            DeleteWorkingFile( uploadFile );

            activity?.Stop( );
        }

        #region PrivateMethods

        /// <summary>
        /// Checks the backup config to determine whether encryption or compression are enabled.<br/>
        /// If either feature is enabled then copy the inputFile to the working directory and return the new FileInfo object.<br/>
        /// Otherwise return the <paramref name="inputFile"/>.
        /// </summary>
        /// <param name="inputFile"></param>
        private FileInfo CopyFileToWorkingDir( FileInfo inputFile ) {
            using Activity? activity = _source.StartActivity( "CopyFileToWorkingDir" )?.Start( );

            FileInfo result = inputFile;

            if (_backupConfig.EncryptBeforeUpload || _backupConfig.CompressBeforeUpload) {
                string filePath = Path.Join( _backupConfig.WorkingDirectory, inputFile.Name );
                _log.LogInformation( "Copying '{string}' to '{string}'.", inputFile.FullName, filePath );
                File.Copy( inputFile.FullName, filePath, true );
                result = new FileInfo( filePath );
            }

            activity?.Stop( );
            return result;
        }


        /// <summary>
        /// Retrieves the Sha512 hash for the <paramref name="inputFile"/> and adds it to the database.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="tabledata"></param>
        private async Task SetOriginalFileHash( FileInfo inputFile, PrimaryTable tabledata ) {
            using Activity? activity = _source.StartActivity( "GetOriginalFileHash" )?.Start( );

            tabledata.FileHash = await _fileHash.GetSha512Hash( inputFile );

            SqliteContext sqliteContext = GetSqliteContext( );
            sqliteContext.Update( tabledata );
            sqliteContext.SaveChanges( );
            ReleaseSqliteContext( );

            activity?.Stop( );
        }


        /// <summary>
        /// Checks whether encryption is enabled. <br/>
        /// If encryption is not enabled then returns the <paramref name="inputFile"/>. <br/>
        /// If encryption is enabled then <br/>
        ///     1. Generate cypherTxtFile FileInfo object. <br/>
        ///     2. Generate random 32 byte crypto key. <br/>
        ///     3. Encrypt <paramref name="inputFile"/> data into cypherTxtFile. <br/>
        ///     4. Add DecryptionData to database. <br/>
        ///     5. Delete <paramref name="inputFile"/>. <br/>
        ///     6. return cypherTxtFile.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="tabledata"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<FileInfo> EncryptFile( FileInfo inputFile, PrimaryTable tabledata ) {
            using Activity? activity = _source.StartActivity( "EncryptFile" )?.Start( );
            FileInfo result = inputFile;

            if (_backupConfig.EncryptBeforeUpload) {
                _log.LogInformation( "Encrypting file '{string}'.", inputFile.FullName );
                if (_crypto == null) {
                    throw new InvalidOperationException( "Cannot encrypt if managed crypto provider is null." );
                }
                FileInfo cypherTxtFile = new( Path.Join( _backupConfig.WorkingDirectory, "encryptedFile.enc" ) );

                byte[] key = RandomNumberGenerator.GetBytes( 32 );
                DecryptionData data = await _crypto.Encrypt( key, inputFile, cypherTxtFile, null );

                // Perform DB work.
                SqliteContext sqliteContext = GetSqliteContext( );
                EncryptionTable? encTableData = sqliteContext.EncryptionData
                    .Where( b => b.Id == tabledata.Id )
                    .FirstOrDefault( );

                if (encTableData == null) {
                    sqliteContext.Add( new EncryptionTable( tabledata.Id, data ) );
                } else {
                    encTableData.DecryptionData = data.ToString( );
                }
                tabledata.IsEncrypted = true;
                sqliteContext.SaveChanges( );
                ReleaseSqliteContext( );

                // Remove plaintext file.
                inputFile.Delete( );
                result = cypherTxtFile;
            }

            activity?.Stop( );
            return result;
        }


        /// <summary>
        /// Checks whether compression is enabled. <br/>
        /// If compression is not enabled then returns the <paramref name="inputFile"/>. <br/>
        /// If compression is enabled then <br/>
        ///     1. Optionally create a Unique Compression Passwords. <br/>
        ///     2. Compress <paramref name="inputFile"/> using 7z CompressionInterface. <br/>
        ///     3. Add compression data to database. <br/>
        ///     4. Delete <paramref name="inputFile"/>. <br/>
        ///     5. return compressed FileInfo object.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="tabledata"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private FileInfo CompressFile( FileInfo inputFile, PrimaryTable tabledata ) {
            using Activity? activity = _source.StartActivity( "CompressFile" )?.Start( );

            FileInfo result = inputFile;

            if (_backupConfig.CompressBeforeUpload) {
                _log.LogInformation( "Compressing '{string}'.", inputFile );
                if (_compress == null) {
                    throw new InvalidOperationException(
                        "Cannot compress before upload if comprssion tool is null."
                    );
                }

                string? password = _backupConfig.UniqueCompressionPasswords ? UniquePassword.Create( ) : null;

                result = _compress.CompressPath( inputFile, password );

                SqliteContext sqliteContext = GetSqliteContext( );
                CompressionTable? compTableData = sqliteContext.CompressionData
                    .Where( b => b.Id == tabledata.Id )
                    .FirstOrDefault( );

                if (compTableData == null) {
                    sqliteContext.Add(
                        new CompressionTable(
                            id: tabledata.Id,
                            passwordProtected: string.IsNullOrWhiteSpace( password ) == false,
                            password: password
                        ) );
                } else {
                    compTableData.PasswordProtected = string.IsNullOrWhiteSpace( password ) == false;
                    compTableData.Password = password;
                }
                tabledata.IsCompressed = true;
                sqliteContext.SaveChanges( );
                ReleaseSqliteContext( );

                // Remove plaintext file.
                inputFile.Delete( );
            }

            activity?.Stop( );
            return result;
        }


        /// <summary>
        /// Retrieves the Sha512 hash for the <paramref name="uploadFile"/> and adds it to the database.
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <param name="tabledata"></param>
        private async Task<string> SetUploadFileHash( FileInfo uploadFile, PrimaryTable tabledata ) {
            using Activity? activity = _source.StartActivity( "SetUploadFileHash" )?.Start( );

            string sha512filehash = await _fileHash.GetSha512Hash( uploadFile );
            tabledata.UploadedFileHash = sha512filehash;

            SqliteContext sqliteContext = GetSqliteContext( );
            sqliteContext.Update( tabledata );
            sqliteContext.SaveChanges( );
            ReleaseSqliteContext( );

            activity?.Stop( );
            return sha512filehash;
        }


        /// <summary>
        /// 1. If ObfuscateUploadedFileNames is enabled the <paramref name="sha512Hash"/> is hashed again and is used in place of the <paramref name="uploadPath"/>. <br/>
        /// 2. <paramref name="uploadFile"/> is uploaded using the BackBlaze Api. <br/>
        /// 3. Backblaze file information is added to the database.
        /// </summary>
        /// <param name="tabledata"></param>
        /// <param name="uploadFile"></param>
        /// <param name="fileName"></param>
        /// <param name="uploadPath"></param>
        /// <param name="sha512Hash"></param>
        private async Task UploadFileToB2(
            PrimaryTable tabledata,
            FileInfo uploadFile,
            string fileName,
            string uploadPath,
            string sha512Hash
        ) {
            using Activity? activity = _source.StartActivity( "UploadFileToB2" )?.Start( );

            if (_backupConfig.ObfuscateUploadedFileNames) {
                uploadPath = _fileHash.GetSha512Hash( sha512Hash );
            }

            _log.LogInformation( "Uploading File To BackBlaze." );
            string fileId = await _backBlaze.UploadFile( uploadFile, fileName, uploadPath, sha512Hash );

            SqliteContext sqliteContext = GetSqliteContext( );
            BackBlazeB2Table? b2TableData = sqliteContext.BackBlazeB2Data
                                            .Where( b => b.Id == tabledata.Id )
                                            .FirstOrDefault( );
            if (b2TableData == null) {
                b2TableData = new BackBlazeB2Table(
                    tabledata.Id,
                    _backblazeConfig.BucketName,
                    _backblazeConfig.BucketId,
                    fileId
                );
                sqliteContext.Add( b2TableData );
                sqliteContext.SaveChanges( );
            } else {
                b2TableData.FileID = fileId;
                b2TableData.BucketName = _backblazeConfig.BucketName;
                b2TableData.BucketId = _backblazeConfig.BucketId;
                sqliteContext.SaveChanges( );
            }
            tabledata.UsesBackBlazeB2 = true;
            sqliteContext.SaveChanges( );
            ReleaseSqliteContext( );

            _log.LogInformation( "UploadFileToB2 DB Data:\n{string}", b2TableData );

            activity?.Stop( );
        }


        /// <summary>
        /// If encryption or compression are enabled then delete <paramref name="uploadFile"/>.
        /// </summary>
        /// <param name="uploadFile"></param>
        private void DeleteWorkingFile( FileInfo uploadFile ) {
            using Activity? activity = _source.StartActivity( "DeleteWorkingFile" )?.Start( );
            if (_backupConfig.EncryptBeforeUpload || _backupConfig.CompressBeforeUpload) { uploadFile.Delete( ); }
            activity?.Stop( );
        }


        /// <summary>
        /// Waits for the semaphore to become available then returns the SqliteContext.
        /// </summary>
        private SqliteContext GetSqliteContext( ) {
            using Activity? activity = _source.StartActivity( "GetSqliteContext" )?.Start( );
            _semaphore.Wait( );
            SqliteContext result = _services.Services.GetRequiredService<SqliteContext>( );
            activity?.Stop( );
            return result;
        }


        /// <summary>
        /// Releases the semaphore so the next sqlite transaction can occur.
        /// </summary>
        private void ReleaseSqliteContext( ) {
            using Activity? activity = _source.StartActivity( "ReleaseSqliteContext" )?.Start( );
            _semaphore.Release( );
            activity?.Stop( );
        }

        #endregion PrivateMethods
    }
}
