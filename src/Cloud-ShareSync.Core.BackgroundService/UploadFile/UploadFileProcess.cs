using System.Diagnostics;
using System.Security.Cryptography;
using Cloud_ShareSync.Core.BackgroundService.PrepFile;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2;
using Cloud_ShareSync.Core.CloudProvider.Types;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Compression.Interfaces;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption;
using Cloud_ShareSync.Core.Cryptography.FileEncryption.Types;
using Cloud_ShareSync.Core.Database;
using Cloud_ShareSync.Core.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.BackgroundService.UploadFile {
    internal class UploadFileProcess : IUploadFileProcess {

        #region Fields

        private static readonly ActivitySource s_source = new( "UploadFileProcess" );

        private static readonly object s_lock = new( );
        private static ICompression? s_compress;

        private readonly ILogger<UploadFileProcess> _log;
        private readonly Hashing _fileHash;
        private readonly ManagedChaCha20Poly1305? _crypto;
        private readonly B2Api _backBlaze;
        private readonly ServiceProvider _services;
        private readonly SyncConfig _backupConfig;
        private readonly B2Config _backblazeConfig;
        private readonly DatabaseConfig _databaseConfig;
        private readonly SemaphoreSlim _semaphore = new( 0, 1 );

        private long _consecutiveExceptionCount;

        #endregion Fields

        public UploadFileProcess(
            SyncConfig backupConfig,
            B2Config backblazeConfig,
            DatabaseConfig databaseConfig,
            CompressionConfig? compressionConfig,
            ILogger<UploadFileProcess> log
        ) {
            _backupConfig = backupConfig;
            _backblazeConfig = backblazeConfig;
            _databaseConfig = databaseConfig;
            _log = log;
            lock (s_lock) {
                if (
                    _backupConfig.CompressBeforeUpload == true &&
                    compressionConfig != null &&
                    s_compress == null
                ) {
                    s_compress = new ManagedCompression( compressionConfig.DependencyPath, _log );
                }
            }
            _fileHash = new( _log );
            _services = new DatabaseServices( _databaseConfig.SqliteDBPath, _log ).Services;
            _ = _semaphore.Release( 1 );
            _backBlaze = new(
                _backblazeConfig.ApplicationKeyId,
                _backblazeConfig.ApplicationKey,
                _backblazeConfig.BucketName,
                _backblazeConfig.BucketId,
                _backblazeConfig.MaxConsecutiveErrors,
                _backblazeConfig.ProcessThreads,
                _log
            );
            _crypto = (backupConfig.EncryptBeforeUpload) ? new( _log ) : null;
        }


        /// <summary>
        /// The high level/abstracted upload file process.
        /// </summary>
        public async Task Process( ) {
            using Activity? activity = s_source.StartActivity( "Process" )?.Start( );

            _log.LogDebug( "Begin Upload File Process" );

            while (IPrepFileProcess.Queue.IsEmpty == false || IUploadFileProcess.Queue.IsEmpty == false) {
                _log.LogDebug( "Upload Work Process. Queue Count: {int}", IUploadFileProcess.Queue.Count );
                bool deQueue = IUploadFileProcess.Queue.TryDequeue( out UploadFileInput? ufInput );
                if (deQueue && ufInput != null) {
                    _log.LogInformation(
                        "Begin upload file process for '{string}'.",
                        ufInput.TableData.FileName
                    );
                    try {
                        FileInfo uploadFile = ufInput.UploadFile.FilePath;
                        string uploadPath = ufInput.UploadFile.UploadFilePath;
                        PrimaryTable tabledata = ufInput.TableData;
                        _log.LogDebug( "PrimaryTabledata:\n{string}", tabledata );

                        uploadFile = CopyFileToWorkingDir( uploadFile );

                        SetOriginalFileHash( ufInput.UploadFile.SHA512, tabledata );

                        uploadFile = await EncryptFile( uploadFile, tabledata );

                        uploadFile = CompressFile( uploadFile, tabledata );

                        await SetUploadFileHash(
                            uploadFile,
                            ufInput.UploadFile.FilePath,
                            ufInput.UploadFile.SHA512,
                            tabledata
                        );

                        // Upload File.
                        await UploadFileToB2(
                            tabledata,
                            uploadFile,
                            ufInput.UploadFile,
                            tabledata.UploadedFileHash // Use current Sha512hash as base for filename obfuscation.
                        );
                        _log.LogInformation( "File Uploaded Successfully." );
                        _log.LogDebug( "PrimaryTabledata:\n{string}", tabledata );

                        // Remove file from working directory (if needed).
                        DeleteWorkingFile( uploadFile );

                        _log.LogInformation(
                            "Completed upload file process for '{string}'.",
                            ufInput.TableData.FileName
                        );
                        _ = Interlocked.Exchange( ref _consecutiveExceptionCount, 0 );
                    } catch (Exception ex) {
                        _log.LogError(
                            "An error occurred during the upload file process. Error: {exception}",
                            ex
                        );
                        _log.LogWarning( "Consecutive Exception Count: {int}", Interlocked.Read( ref _consecutiveExceptionCount ) );
                        if (Interlocked.Read( ref _consecutiveExceptionCount ) >= 5) {
                            string aggMsg = "Upload file process has received too many consecutive errors. " +
                                "Aborting to avoid an infinite error loop.";
                            _log.LogCritical( "{string}\n{exception}", aggMsg, ex );
                            // throwing the exception below isn't killing the app?!?
                            Environment.Exit( 200 ); // Setting exit works.
                            //throw new AggregateException( aggMsg, ex );
                        } else {
                            _log.LogInformation(
                                "Re-enqueueing '{string}' for later re-processing.",
                                ufInput.UploadFile.FilePath.FullName
                            );
                            IUploadFileProcess.Queue.Enqueue( ufInput );
                            _ = Interlocked.Increment( ref _consecutiveExceptionCount );
                            _log.LogInformation( "Sleeping for 30 seconds after failure." );
                            await Task.Delay( 30 * 1000 );
                        }
                    }
                } else {
                    _log.LogDebug( "UploadFileProcess Dequeue either failed, or was null." );
                    await Task.Delay( 1500 ); // wait a second and a half before checking the queue again.
                }
            }

            _log.LogDebug( "Upload Work Process Completed. Queue Count: {int}", IUploadFileProcess.Queue.Count );
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
            using Activity? activity = s_source.StartActivity( "CopyFileToWorkingDir" )?.Start( );

            FileInfo result = inputFile;

            if (_backupConfig.EncryptBeforeUpload || _backupConfig.CompressBeforeUpload) {
                _log.LogDebug( "Copying '{string}' to Working Dir.", inputFile.FullName );
                string filePath = Path.Join( _backupConfig.WorkingDirectory, inputFile.Name );
                _log.LogInformation( "Copying '{string}' to '{string}'.", inputFile.FullName, filePath );
                File.Copy( inputFile.FullName, filePath, true );
                result = new FileInfo( filePath );
                _log.LogDebug( "Copied File to Working Dir." );
            } else {
                _log.LogDebug( "File will be processed in place '{string}'.", inputFile.FullName );
            }

            activity?.Stop( );
            return result;
        }


        /// <summary>
        /// Retrieves the Sha512 hash for the <paramref name="inputFile"/> and adds it to the database.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="tabledata"></param>
        private void SetOriginalFileHash( string sha512, PrimaryTable tabledata ) {
            using Activity? activity = s_source.StartActivity( "GetOriginalFileHash" )?.Start( );
            _log.LogInformation( "Setting original file hash for '{string}'.", tabledata.FileName );

            tabledata.FileHash = sha512;

            SqliteContext sqliteContext = GetSqliteContext( );
            _ = sqliteContext.Update( tabledata );
            _ = sqliteContext.SaveChanges( );
            ReleaseSqliteContext( );
            _log.LogInformation(
                "Set original file hash for '{string}' to {string}",
                tabledata.FileName,
                tabledata.FileHash
            );

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
        /// <exception cref="ApplicationException"></exception>
        private async Task<FileInfo> EncryptFile( FileInfo inputFile, PrimaryTable tabledata ) {
            using Activity? activity = s_source.StartActivity( "EncryptFile" )?.Start( );
            FileInfo result = inputFile;

            if (_backupConfig.EncryptBeforeUpload) {
                if (_crypto == null) {
                    throw new ApplicationException( "Cannot encrypt if managed crypto provider is null." );
                }

                FileInfo cypherTxtFile = new( Path.Join( _backupConfig.WorkingDirectory, Path.GetRandomFileName( ) ) );

                byte[] key = RandomNumberGenerator.GetBytes( 32 );
                ManagedChaCha20Poly1305DecryptionData data = await _crypto.Encrypt( key, inputFile, cypherTxtFile, null );

                // Perform DB work.
                SqliteContext sqliteContext = GetSqliteContext( );
                EncryptionTable? encTableData = sqliteContext.EncryptionData
                    .Where( b => b.Id == tabledata.Id )
                    .FirstOrDefault( );

                if (encTableData == null) {
                    _ = sqliteContext.Add( new EncryptionTable( tabledata.Id, data ) );
                } else {
                    encTableData.DecryptionData = data.ToString( );
                }
                tabledata.IsEncrypted = true;
                _ = sqliteContext.SaveChanges( );
                ReleaseSqliteContext( );

                // Remove plaintext file.
                inputFile.Delete( );
                result = cypherTxtFile;
            } else {
                _log.LogInformation( "SyncConfig EncryptBeforeUpload is not enabled. Skipping encryption." );
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
        /// <exception cref="ApplicationException"></exception>
        private FileInfo CompressFile( FileInfo inputFile, PrimaryTable tabledata ) {
            using Activity? activity = s_source.StartActivity( "CompressFile" )?.Start( );

            FileInfo result = inputFile;

            if (_backupConfig.CompressBeforeUpload) {
                if (s_compress == null) {
                    throw new ApplicationException(
                        "Cannot compress before upload if comprssion tool is null."
                    );
                }

                string? password = _backupConfig.UniqueCompressionPasswords ? UniquePassword.Create( ) : null;
                FileInfo compressionPath = new( Path.Join( _backupConfig.WorkingDirectory, Path.GetRandomFileName( ) ) );

                result = s_compress.CompressPath( inputFile, compressionPath, password ).Result;

                SqliteContext sqliteContext = GetSqliteContext( );
                CompressionTable? compTableData = sqliteContext.CompressionData
                    .Where( b => b.Id == tabledata.Id )
                    .FirstOrDefault( );

                if (compTableData == null) {
                    _ = sqliteContext.Add(
                        new CompressionTable(
                            id: tabledata.Id,
                            passwordProtected: string.IsNullOrWhiteSpace( password ) == false,
                            password: password
                        )
                    );
                } else {
                    compTableData.PasswordProtected = string.IsNullOrWhiteSpace( password ) == false;
                    compTableData.Password = password;
                }
                tabledata.IsCompressed = true;
                _ = sqliteContext.SaveChanges( );
                ReleaseSqliteContext( );

                // Remove plaintext file.
                inputFile.Delete( );
            } else {
                _log.LogInformation( "SyncConfig CompressBeforeUpload is not enabled. Skipping compression." );
            }

            activity?.Stop( );
            return result;
        }


        /// <summary>
        /// Retrieves the Sha512 hash for the <paramref name="uploadFile"/> and adds it to the database.
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <param name="tabledata"></param>
        private async Task SetUploadFileHash(
            FileInfo uploadFile,
            FileInfo preProcessFile,
            string preProcessSha512,
            PrimaryTable tabledata
        ) {
            using Activity? activity = s_source.StartActivity( "SetUploadFileHash" )?.Start( );

            _log.LogInformation( "Setting upload file hash for '{string}'.", tabledata.FileName );
            tabledata.UploadedFileHash = uploadFile == preProcessFile
                ? preProcessSha512
                : await _fileHash.GetSha512Hash( uploadFile );

            SqliteContext sqliteContext = GetSqliteContext( );
            _ = sqliteContext.Update( tabledata );
            _ = sqliteContext.SaveChanges( );
            ReleaseSqliteContext( );

            _log.LogInformation(
                "Set upload file hash for '{string}' to {string}.",
                tabledata.FileName,
                tabledata.UploadedFileHash
            );

            activity?.Stop( );
        }


        /// <summary>
        /// 1. If ObfuscateUploadedFileNames is enabled the <paramref name="sha512Hash"/> is hashed again and is used in place of the <paramref name="uploadPath"/>. <br/>
        /// 2. <paramref name="uploadFile"/> is uploaded using the BackBlaze Api. <br/>
        /// 3. Backblaze file information is added to the database.
        /// </summary>
        /// <param name="tabledata"></param>
        /// <param name="uploadFile"></param>
        /// <param name="uFileInfo"></param>
        /// <param name="sha512Hash"></param>
        private async Task UploadFileToB2(
            PrimaryTable tabledata,
            FileInfo uploadFile,
            UploadFileInfo uFileInfo,
            string sha512Hash
        ) {
            using Activity? activity = s_source.StartActivity( "UploadFileToB2" )?.Start( );

            uFileInfo.FilePath = uploadFile;
            uFileInfo.SHA1 = await _fileHash.GetSha1Hash( uploadFile );
            if (_backupConfig.ObfuscateUploadedFileNames) {
                _log.LogInformation( "SyncConfig ObfuscateUploadedFileNames is enabled. Hashing original sha512Hash to use instead." );
                uFileInfo.UploadFilePath = _fileHash.GetSha512Hash( sha512Hash );
            }

            _log.LogDebug( "UploadFileProcess - Uploading File to B2." );
            string fileId = await _backBlaze.UploadFile( uFileInfo );

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
                _ = sqliteContext.Add( b2TableData );
                _ = sqliteContext.SaveChanges( );
            } else {
                b2TableData.FileID = fileId;
                b2TableData.BucketName = _backblazeConfig.BucketName;
                b2TableData.BucketId = _backblazeConfig.BucketId;
                _ = sqliteContext.SaveChanges( );
            }
            tabledata.StoredInBackBlazeB2 = true;
            _ = sqliteContext.SaveChanges( );
            ReleaseSqliteContext( );

            _log.LogDebug( "UploadFileToB2 DB Data:\n{string}", b2TableData );

            activity?.Stop( );
        }


        /// <summary>
        /// If encryption or compression are enabled then delete <paramref name="uploadFile"/>.
        /// </summary>
        /// <param name="uploadFile"></param>
        private void DeleteWorkingFile( FileInfo uploadFile ) {
            using Activity? activity = s_source.StartActivity( "DeleteWorkingFile" )?.Start( );
            if (_backupConfig.EncryptBeforeUpload || _backupConfig.CompressBeforeUpload) { uploadFile.Delete( ); }
            activity?.Stop( );
        }


        /// <summary>
        /// Waits for the semaphore to become available then returns the SqliteContext.
        /// </summary>
        private SqliteContext GetSqliteContext( ) {
            using Activity? activity = s_source.StartActivity( "GetSqliteContext" )?.Start( );
            _semaphore.Wait( );
            SqliteContext result = _services.GetRequiredService<SqliteContext>( );
            activity?.Stop( );
            return result;
        }


        /// <summary>
        /// Releases the semaphore so the next sqlite transaction can occur.
        /// </summary>
        private void ReleaseSqliteContext( ) {
            using Activity? activity = s_source.StartActivity( "ReleaseSqliteContext" )?.Start( );
            _ = _semaphore.Release( );
            activity?.Stop( );
        }

        #endregion PrivateMethods
    }
}
