using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Compression.Interfaces;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography.FileEncryption;
using Cloud_ShareSync.Core.Cryptography.FileEncryption.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.BackgroundService.DownloadFile {
    internal class DownloadFileProcess : IDownloadFileProcess {

        #region Fields

        private static readonly ActivitySource s_source = new( "DownloadFileProcess" );

        private static readonly object s_lock = new( );
        private static ICompression? s_compress;

        private readonly ILogger<DownloadFileProcess> _log;
        private readonly ManagedChaCha20Poly1305? _crypto;
        private readonly B2Api _backBlaze;
        private readonly SyncConfig _syncConfig;
        private readonly B2Config _backblazeConfig;
        private List<B2File> _b2Files;
        private DateTime _lastRetrieved;
        private long _consecutiveExceptionCount;

        #endregion Fields

        public DownloadFileProcess(
            SyncConfig syncConfig,
            B2Config backblazeConfig,
            CompressionConfig? compressionConfig,
            ILogger<DownloadFileProcess> log
        ) {
            _syncConfig = syncConfig;
            _backblazeConfig = backblazeConfig;
            _log = log;
            lock (s_lock) {
                if (
                    compressionConfig != null &&
                    s_compress == null
                ) {
                    s_compress = new ManagedCompression( compressionConfig.DependencyPath, _log );
                }
            }
            _backBlaze = new(
                _backblazeConfig.ApplicationKeyId,
                _backblazeConfig.ApplicationKey,
                _backblazeConfig.BucketName,
                _backblazeConfig.BucketId,
                _backblazeConfig.MaxConsecutiveErrors,
                _backblazeConfig.ProcessThreads,
                _log
            );
            _crypto = syncConfig.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) ? new( _log ) : null;
            _lastRetrieved = DateTime.Now.AddMinutes( -5 );
            _b2Files = GetB2FileList( ).Result;
        }


        /// <summary>
        /// The high level/abstracted Download file process.
        /// </summary>
        public async Task Process( ) {
            using Activity? activity = s_source.StartActivity( "Process" )?.Start( );

            _log.LogDebug( "Begin Download File Process" );

            while (IDownloadFileProcess.Queue.IsEmpty == false) {
                _log.LogDebug( "Download Work Process. Queue Count: {int}", IDownloadFileProcess.Queue.Count );
                bool deQueue = IDownloadFileProcess.Queue.TryDequeue( out DownloadFileInput? dfInput );
                if (deQueue && dfInput != null) {
                    try {
                        _log.LogInformation(
                            "Begin download file process for '{string}'.",
                            dfInput.FilePath.FullName
                        );
                        _log.LogDebug( "PrimaryTabledata:\n{string}", dfInput.TableData );
                        FileInfo downloadedFile = await DownloadFileFromB2( dfInput );
                        downloadedFile = await DecompressDownloadedFile( dfInput, downloadedFile );
                        downloadedFile = await DecryptDownloadedFile( dfInput, downloadedFile );
                        MoveFileToFinalPath( downloadedFile, dfInput.FilePath );
                        _log.LogInformation(
                            "Completed download file process for '{string}'.",
                            dfInput.FilePath.FullName
                        );
                        _ = Interlocked.Exchange( ref _consecutiveExceptionCount, 0 );
                    } catch (Exception ex) {
                        _log.LogError(
                            "An error occurred during the download file process. Error: {exception}",
                            ex
                        );
                        _log.LogWarning( "Consecutive Exception Count: {int}", Interlocked.Read( ref _consecutiveExceptionCount ) );
                        if (Interlocked.Read( ref _consecutiveExceptionCount ) >= 5) {
                            string aggMsg = "Download file process has received too many consecutive errors. " +
                                "Aborting to avoid an infinite error loop.";
                            _log.LogCritical( "{string}\n{exception}", aggMsg, ex );
                            Environment.Exit( 200 );
                        } else {
                            _log.LogInformation(
                                "Re-enqueueing '{string}' for later re-processing.",
                                dfInput.FilePath.FullName
                            );
                            IDownloadFileProcess.Queue.Enqueue( dfInput );
                            _ = Interlocked.Increment( ref _consecutiveExceptionCount );
                            _log.LogInformation( "Sleeping for 30 seconds after failure." );
                            await Task.Delay( 30 * 1000 );
                        }
                    }
                }
            }

            _log.LogDebug( "Download Work Process Completed. Queue Count: {int}", IDownloadFileProcess.Queue.Count );
            activity?.Stop( );
        }

        private async Task<FileInfo> DownloadFileFromB2( DownloadFileInput dfInput ) {
            FileInfo downloadPath = new( Path.Join( _syncConfig.WorkingDirectory, Path.GetRandomFileName( ) ) );
            List<B2File> fileResponse = await GetB2FileList( );
            B2File? file = fileResponse.FirstOrDefault( e => e.fileId == dfInput.BackBlazeData!.FileID );
            await _backBlaze.DownloadFileId(
                dfInput.BackBlazeData!.FileID,
                downloadPath,
                file?.contentLength,
                dfInput.TableData.UploadedFileHash
            );
            return downloadPath;
        }

        private async Task<FileInfo> DecompressDownloadedFile( DownloadFileInput dfInput, FileInfo downloadedFile ) {
            if (dfInput.CompressionData != null) {
                DirectoryInfo decompressionDir = Directory.CreateDirectory( Path.Join( _syncConfig.WorkingDirectory, Path.GetRandomFileName( ) ) );
                IEnumerable<FileSystemInfo> decompressedItems = await s_compress!.DecompressPath(
                    downloadedFile,
                    decompressionDir,
                    dfInput.CompressionData.Password
                );
                if (decompressedItems.Count( ) == 1) {
                    downloadedFile.Delete( );
                    return (FileInfo)decompressedItems.First( );
                } else {
                    throw new ApplicationException( "Should only have one file per compressed download." );
                }
            }

            _log.LogInformation( "Downloaded file is not compressed. Skipping decompression." );
            return downloadedFile;
        }

        private async Task<FileInfo> DecryptDownloadedFile( DownloadFileInput dfInput, FileInfo downloadedFile ) {
            if (dfInput.EncryptionData != null) {
                FileInfo plaintxtFile = new( Path.Join( _syncConfig.WorkingDirectory, Path.GetRandomFileName( ) ) );
                await _crypto!.Decrypt(
                    ManagedChaCha20Poly1305DecryptionData.Deserialize( dfInput.EncryptionData.DecryptionData ),
                    downloadedFile,
                    plaintxtFile
                );
                return plaintxtFile;
            }
            _log.LogInformation( "Downloaded file is not encrypted. Skipping decryption." );
            return downloadedFile;
        }

        private void MoveFileToFinalPath( FileInfo downloadedFile, FileInfo finalPath ) =>
            File.Move( downloadedFile.FullName, finalPath.FullName, true );

        private async Task<List<B2File>> GetB2FileList( ) {
            using Activity? activity = s_source.StartActivity( "GetB2FileList" )?.Start( );
            if (_lastRetrieved < DateTime.Now.AddMinutes( -1 )) {
                _b2Files = await _backBlaze.ListBucketFiles( );
                _lastRetrieved = DateTime.Now;
            }
            activity?.Stop( );
            return _b2Files;
        }

    }
}
