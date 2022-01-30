using System.Diagnostics;
using System.Security.Cryptography;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Configuration.Types.Cloud;
using Cloud_ShareSync.Core.Configuration.Types.Features;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption;
using Cloud_ShareSync.Core.Cryptography.FileEncryption.Types;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleBackup.BackupService {
    internal class UploadFileProcess : IUploadFileProcess {

        #region Fields

        private readonly ActivitySource _source = new( "UploadFileProcess" );
        private readonly BackupConfig _backupConfig;
        private readonly B2Config _backblazeConfig;
        private readonly DatabaseConfig _databaseConfig;
        private readonly CompressionConfig? _compressionConfig;
        private readonly ILogger? _log;
        private readonly FileHash _fileHash;
        private readonly CloudShareSyncServices _services;
        private readonly SemaphoreSlim _semaphore = new( 0, 1 );
        private readonly BackBlazeB2 _backBlaze;
        private readonly ManagedChaCha20Poly1305? _crypto;

        #endregion Fields

        public UploadFileProcess(
            BackupConfig backupConfig,
            B2Config backblazeConfig,
            DatabaseConfig databaseConfig,
            CompressionConfig? compressionConfig,
            ILogger? log = null
        ) {
            _backupConfig = backupConfig;
            _backblazeConfig = backblazeConfig;
            _databaseConfig = databaseConfig;
            _compressionConfig = compressionConfig;
            _log = log;
            _fileHash = new( _log );
            _services = new CloudShareSyncServices( _databaseConfig.SqliteDBPath, _log );
            _semaphore.Release( 1 );
            _backBlaze = new( _backblazeConfig, _log );
            _crypto = (backupConfig.EncryptBeforeUpload) ? new( _log ) : null;
        }


        public async Task Process( FileInfo uploadFile, string uploadPath, PrimaryTable tabledata ) {
            using Activity? activity = _source.StartActivity( "UploadFileProcess" )?.Start( );

            // Determine whether to copy file to the working dir for processing.
            uploadFile = CopyFileToWorkingDir( uploadFile );

            // Conditionally encrypt file before upload.
            uploadFile = await EncryptFile( uploadFile, tabledata );

            // Conditionally compress file before upload.
            uploadFile = CompressFile( uploadFile, tabledata );

            string sha512filehash = await SetUploadFileHash( uploadFile, tabledata );

            // Upload File.
            await UploadFileToB2(
                tabledata,
                uploadFile,
                uploadFile.FullName,
                uploadPath,
                sha512filehash
            );

            // Remove file from working directory (if needed).
            DeleteWorkingFile( uploadFile );

            activity?.Stop( );
        }

        #region PrivateMethods

        private FileInfo CopyFileToWorkingDir( FileInfo uploadFile ) {
            using Activity? activity = _source.StartActivity( "CopyFileToWorkingDir" )?.Start( );

            FileInfo result = uploadFile;

            if (_backupConfig.EncryptBeforeUpload || _backupConfig.CompressBeforeUpload) {
                string filePath = Path.Join( _backupConfig.WorkingDirectory, uploadFile.Name );
                _log?.LogInformation( "Copying '{string}' to '{string}'.", uploadFile.FullName, filePath );
                File.Copy( uploadFile.FullName, filePath, true );
                result = new FileInfo( filePath );
            }

            activity?.Stop( );
            return result;
        }

        private async Task<FileInfo> EncryptFile( FileInfo uploadFile, PrimaryTable tabledata ) {
            using Activity? activity = _source.StartActivity( "EncryptFile" )?.Start( );
            _log?.LogInformation( "Encrypting file '{string}'.", uploadFile.FullName );

            FileInfo result = uploadFile;

            if (_backupConfig.EncryptBeforeUpload) {
                if (_crypto == null) {
                    throw new InvalidOperationException( "Cannot encrypt if managed crypto provider is null." );
                }
                FileInfo cypherTxtFile = new( Path.Join( _backupConfig.WorkingDirectory, "encryptedFile.enc" ) );

                byte[] key = RandomNumberGenerator.GetBytes( 32 );
                DecryptionData data = await _crypto.Encrypt( key, uploadFile, cypherTxtFile, null );

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
                uploadFile.Delete( );
                result = cypherTxtFile;
            }

            activity?.Stop( );
            return result;
        }

        private FileInfo CompressFile( FileInfo uploadFile, PrimaryTable tabledata ) {
            using Activity? activity = _source.StartActivity( "CompressFile" )?.Start( );

            FileInfo result = uploadFile;

            if (_backupConfig.CompressBeforeUpload) {
                _log?.LogInformation( "Compressing '{string}'.", uploadFile );

                string? password = _backupConfig.UniqueCompressionPasswords ? UniquePassword.Create( ) : null;
                string? decompressionargs = _compressionConfig?.DeCompressionCmdlineArgs;

                result = CompressionInterface.CompressPath( uploadFile, password );

                SqliteContext sqliteContext = GetSqliteContext( );
                CompressionTable? compTableData = sqliteContext.CompressionData
                    .Where( b => b.Id == tabledata.Id )
                    .FirstOrDefault( );

                if (compTableData == null) {
                    sqliteContext.Add(
                        new CompressionTable(
                            id: tabledata.Id,
                            passwordProtected: string.IsNullOrWhiteSpace( password ) == false,
                            password: password,
                            specialDecompress: string.IsNullOrWhiteSpace( decompressionargs ) == false,
                            decompressionArgs: decompressionargs
                        ) );
                } else {
                    compTableData.PasswordProtected = string.IsNullOrWhiteSpace( password ) == false;
                    compTableData.Password = password;
                    compTableData.SpecialDecompress = string.IsNullOrWhiteSpace( decompressionargs ) == false;
                    compTableData.DecompressionArgs = decompressionargs;
                }
                tabledata.IsCompressed = true;
                sqliteContext.SaveChanges( );
                ReleaseSqliteContext( );

                // Remove plaintext file.
                uploadFile.Delete( );
            }

            activity?.Stop( );
            return result;
        }

        private async Task<string> SetUploadFileHash( FileInfo uploadFile, PrimaryTable tabledata ) {
            using Activity? activity = _source.StartActivity( "SetUploadFileHash" )?.Start( );

            string sha512filehash = await _fileHash.GetSha512FileHash( uploadFile );
            if (_backupConfig.ObfuscateUploadedFileNames) {
                sha512filehash = _fileHash.GetSha512StringHash( sha512filehash );
            }
            tabledata.UploadedFileHash = sha512filehash;

            SqliteContext sqliteContext = GetSqliteContext( );
            sqliteContext.Update( tabledata );
            sqliteContext.SaveChanges( );
            ReleaseSqliteContext( );

            activity?.Stop( );
            return sha512filehash;
        }

        private async Task UploadFileToB2(
            PrimaryTable tabledata,
            FileInfo uploadFile,
            string fileName,
            string uploadPath,
            string sha512Hash
        ) {
            using Activity? activity = _source.StartActivity( "UploadFileToB2" )?.Start( );

            if (_backupConfig.ObfuscateUploadedFileNames) { uploadPath = sha512Hash; }

            _log?.LogInformation( "Uploading File To BackBlaze." );
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

            _log?.LogInformation( "UploadFileToB2 DB Data:\n{string}", b2TableData );

            activity?.Stop( );
        }

        private void DeleteWorkingFile( FileInfo uploadFile ) {
            using Activity? activity = _source.StartActivity( "DeleteWorkingFile" )?.Start( );
            if (_backupConfig.EncryptBeforeUpload || _backupConfig.CompressBeforeUpload) { uploadFile.Delete( ); }
            activity?.Stop( );
        }

        private SqliteContext GetSqliteContext( ) {
            using Activity? activity = _source.StartActivity( "GetSqliteContext" )?.Start( );
            _semaphore.Wait( );
            SqliteContext result = _services.Services.GetRequiredService<SqliteContext>( );
            activity?.Stop( );
            return result;
        }

        private void ReleaseSqliteContext( ) {
            using Activity? activity = _source.StartActivity( "ReleaseSqliteContext" )?.Start( );
            _semaphore.Release( );
            activity?.Stop( );
        }

        #endregion PrivateMethods
    }
}
