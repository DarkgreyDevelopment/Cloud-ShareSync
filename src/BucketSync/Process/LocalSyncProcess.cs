using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption;
using Cloud_ShareSync.Core.Cryptography.FileEncryption.Types;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.FileSystemWatcher;
using Microsoft.Extensions.Options;
/*
namespace Cloud_ShareSync.BucketSync.Process {

    public class LocalSyncProcess : ILocalSyncProcess {

        private readonly ActivitySource _source = new( "LocalSyncProcess" );
        private readonly BackupConfig _config;
        private readonly ILogger<LocalSyncProcess> _logger;
        private bool _startupCompleted;
        private FileWatch? _watcher;
        private readonly FileHash _fileHash;
        private readonly ManagedChaCha20Poly1305 _crypto;

        public LocalSyncProcess(
            IOptions<BackupConfig> config,
            ILogger<LocalSyncProcess> logger
        ) {
            _config = config.Value;
            _logger = logger;
            _fileHash = new( ); // Need to pass in ILog to get filehash messages.
            _crypto = new( );
        }

        public void Startup( ) {
            if (_startupCompleted) { return; }
            using Activity? activity = _source.StartActivity( "Startup" )?.Start( );

            if (Directory.Exists( _config.WorkingDirectory )) {
                _logger.LogInformation( "Working Directory Exists" );
            } else {
                activity?.Stop( );
                throw new DirectoryNotFoundException( $"Working directory '{_config.WorkingDirectory}' doesn't exist." );
            }

            _watcher = new(
                new DirectoryInfo( _config.RootFolder ),
                null,
                null,
                _config.MonitorSubDirectories
            );

            int count = 0;
            foreach (string file in GetRootFiles( _config.RootFolder, _config.MonitorSubDirectories )) {
                _logger.LogInformation( "Enqueueing File{int}: {string}", count, file );
                FileWatch.UniqueEnqueue( _watcher._createdEvents, new( file ) );
                count++;
            }
            _startupCompleted = true;
            activity?.Stop( );
        }

        public async Task Process( ) {
            using Activity? activity = _source.StartActivity( "Process" )?.Start( );

            _logger.LogInformation( "Worker running Process at: {time}", DateTimeOffset.Now );

            if (_watcher == null) { throw new Exception( "Shits Broke Yo!" ); }

            List<Exception> expList = new( );
            foreach (KeyValuePair<FileSystemWatcher, Exception> err in _watcher._errorEvents) {
                expList.Add( err.Value );
            }

            if (expList.Count > 0) { throw new AggregateException( "FileSystemWatcher had errors.", expList ); }

            ConcurrentBag<Tuple<string, FileInfo, string, string, DecryptionData?, string?>> results = new( );

            // FilePath, Upload FileInfo, Sha512Hash, FileId, DecryptionData, 7Z Compression password

            while (_watcher._createdEvents.IsEmpty == false) {
                bool deQueue = _watcher._createdEvents.TryDequeue( out EventData? cE );

                if (deQueue && cE != null && MatchesExcludedPath( cE.FullPath ) == false) {
                    await UploadFileToB2( cE.FullPath );
                }
            }
            _watcher._changedEvents.Clear( );
            _watcher._renamedEvents.Clear( );
            _watcher._deletedEvents.Clear( );

            activity?.Stop( );
            return;
        }

        private async Task<UploadStaging> UploadFileToB2( string path, PrimaryTable? tabledata = null ) {

            _logger.LogInformation( "Uploading File: {string}", path );

            if (s_backBlaze == null) {
                throw new InvalidOperationException( "Cannot proceed if backblaze configuration is not initialized." );
            }

            // Initialize Required Variables
            string fileId;
            DecryptionData? data = null;
            string uploadPath = Path.GetRelativePath( _config.RootFolder, path );

            string? password = null;
            if (_config.UniqueCompressionPasswords) { password = UniquePassword.Create( ); }

            // Determine whether to copy file to the working dir for processing.
            FileInfo uploadFile = (_config.EncryptBeforeUpload || _config.CompressBeforeUpload) ?
                PrepUpload( path ) : new( path );
            FileInfo originalUploadFile = uploadFile;

            // Get Sha 512 FileHash
            _logger.LogInformation( "Retrieving Sha512 file hash." );
            string sha512filehash = await _fileHash.GetSha512FileHash( uploadFile );

            if (tabledata == null) {
                tabledata = new PrimaryTable(
                    filename: uploadFile.Name,
                    uploadpath: uploadPath,
                    hash: sha512filehash,
                    uploadhash: "",
                    encrypted: false,
                    compressed: false,
                    aws: false,
                    azure: false,
                    backblaze: true,
                    gcs: false
                );
            }

            // Encrypt file before upload.
            if (_config.EncryptBeforeUpload) {
                FileInfo cypherTxtFile = new( Path.Join( _config.WorkingDirectory, sha512filehash ) );
                data = await EncryptFile( uploadFile, cypherTxtFile );
                uploadFile = cypherTxtFile;
                tabledata.IsEncrypted = true;
            }

            // Compress file before upload.
            if (_config.CompressBeforeUpload) {
                uploadFile = CompressFile( uploadFile, password );
                tabledata.IsCompressed = true;
            }

            if (uploadFile == originalUploadFile) {
                tabledata.UploadedFileHash = sha512filehash;
            } else {
                _logger.LogInformation(
                    "Upload file has been compressed or encrypted. Retrieving Sha512 file hash prior to upload."
                );
                tabledata.UploadedFileHash = await _fileHash.GetSha512FileHash( uploadFile );
            }

            // Upload File.
            _logger.LogInformation( "Uploading File To BackBlaze." );
            fileId = await s_backBlaze.UploadFile(
                uploadFile,
                path,
                uploadPath,
                sha512filehash
            );
            BackBlazeB2Table backBlazeResult = new(
                id: tabledata.Id,
                bucketName: "",
                bucketId: "",
                fileID: fileId

            );

            // Remove file from working directory (if needed).
            if (_config.EncryptBeforeUpload || _config.CompressBeforeUpload) { uploadFile.Delete( ); }
            UploadStaging uploadStaging = new( tabledata, backBlazeResult );
            uploadStaging.CompressionData = tabledata.IsCompressed ?
                                                new( tabledata.Id, password == null, password, false, null ) :
                                                null;
            uploadStaging.EncryptionData = data != null ?
                                                new( tabledata.Id, data.ToString( ) ) :
                                                null;

            return uploadStaging;
        }

        private FileInfo CompressFile(
            FileInfo uploadFile,
            string? password
        ) {
            _logger.LogInformation( "Compressing file before upload." );

            FileInfo? compressedFile = CompressionInterface.CompressPath(
                uploadFile,
                password
            );

            // Remove plaintext file.
            uploadFile.Delete( );
            return compressedFile;
        }

        private async Task<DecryptionData> EncryptFile(
            FileInfo uploadFile,
            FileInfo cypherTxtFile
        ) {
            _logger.LogInformation( "Encrypting file before upload." );

            byte[] key = RandomNumberGenerator.GetBytes( 32 );
            DecryptionData data = await _crypto.Encrypt( key, uploadFile, cypherTxtFile, null );

            // Remove plaintext file.
            uploadFile.Delete( );
            return data;
        }


        private FileInfo PrepUpload( string path ) {
            string filePath = Path.Join( _config.WorkingDirectory, new FileInfo( path ).Name );
            _logger.LogInformation( "Copying '{string}' to '{string}'", path, filePath );
            File.Copy( path, filePath, true );
            return new FileInfo( filePath );
        }

        private bool MatchesExcludedPath( string path ) {
            bool matchesExcludedPaths = false;
            foreach (string exPath in _config.ExcludePaths) {
                if (Regex.Match( path, exPath, RegexOptions.IgnoreCase ).Success) {
                    matchesExcludedPaths = true;
                    break;
                }
            }
            return matchesExcludedPaths;
        }

        private static IEnumerable<string> GetRootFiles( string path, bool recursiveSearch ) {
            return Directory.EnumerateFiles(
                path,
                "*",
                (recursiveSearch) ?
                    SearchOption.AllDirectories :
                    SearchOption.TopDirectoryOnly
            );
        }
    }
}
*/
