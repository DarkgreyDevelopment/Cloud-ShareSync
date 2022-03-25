using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.CloudProvider.Interfaces;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {
    internal class BackBlazeB2 : ICloudProvider {

        private static readonly ActivitySource s_source = new( "BackBlazeB2.PublicInterface" );
        private readonly B2? _b2Api;
        private readonly Hashing? _fileHash;
        private readonly ILogger? _logger;
        private readonly int _maxErrors;

        public BackBlazeB2(
            B2Config config,
            ILogger? logger = null
        ) {
            _logger = logger;
            _fileHash = new Hashing( _logger );
            _maxErrors = (config.MaxConsecutiveErrors <= 0) ? 1 : config.MaxConsecutiveErrors; // Requires a minimum of 1.
            _b2Api = new(
                config.ApplicationKeyId,
                config.ApplicationKey,
                _maxErrors,
                (config.ProcessThreads <= 0) ? 1 : config.ProcessThreads, // Requires a minimum of 1.
                config.BucketName,
                config.BucketId,
                _logger
            );
        }

        #region Upload

        // Interface Method - Does not return FileId.
        public void UploadFile( UploadB2File upload ) { _ = UploadFile( upload, _b2Api ).Result; }
        public async Task<string> UploadFile( string path ) => await UploadFile( new FileInfo( path ) );
        public async Task<string> UploadFile( FileInfo path ) => await UploadFile( new UploadB2File( path ), _b2Api );
        public async Task<string> UploadFile(
            FileInfo path,
            string originalFileName,
            string uploadFilePath,
            string sha512Hash
        ) => await UploadFile( new UploadB2File( path, originalFileName, uploadFilePath, sha512Hash ), _b2Api );

        private async Task<string> UploadFile( UploadB2File upload, B2? b2Api ) {
            using Activity? activity = s_source.StartActivity( "UploadFile" )?.Start( );

            if (b2Api == null) {
                activity?.Stop( );
                throw new ApplicationException( "Initialize before uploading." );
            }

            int count = 1;
            bool success = false;
            string result = string.Empty;
            do {
                try {
                    result = await b2Api.UploadFileToBackBlaze( upload );
                    success = true;
                } catch (Exception ex) {
                    if (count == _maxErrors) {
                        _logger?.LogCritical( "Failed to upload file to backblaze.\n{exception}", ex );
                        throw;
                    } else {
                        _logger?.LogError( "Error while uploading file to backblaze.\n{exception}", ex );
                    }
                    count++;

                    if (count < _maxErrors) {
                        _logger?.LogError( "Sleeping for a minute before retrying." );
                        Thread.Sleep( 60000 );
                    }
                }
            } while (count <= _maxErrors && success == false);

            activity?.Stop( );
            return result;
        }

        #endregion Upload


        #region Download

        public void DownloadFile( DownloadB2File download ) {
            using Activity? activity = s_source.StartActivity( "DownloadFile" )?.Start( );

            if (_b2Api == null) {
                activity?.Stop( );
                throw new ApplicationException( "Initialize before downloading." );
            }
            _ = DownloadFile( download, _b2Api ).Result;

            activity?.Stop( );
        }

        internal async Task<bool> DownloadFile( DownloadB2File download, B2 b2Api ) {
            using Activity? activity = s_source.StartActivity( "DownloadFile.Async" )?.Start( );

            if (_fileHash == null) {
                activity?.Stop( );
                throw new ApplicationException( "Initialize before downloading." );
            }

            if (string.IsNullOrWhiteSpace( download.FileId ) == false) {
                B2DownloadResponse response = await b2Api.DownloadFileID( download.FileId, download.OutputPath );
                _logger?.LogDebug( "Download Response: {string}", response );
                if (string.IsNullOrWhiteSpace( response.Sha1FileHash ) == false) {
                    string downloadedSha1Hash = await _fileHash.GetSha1Hash( response.OutputPath );
                    if (response.Sha1FileHash != downloadedSha1Hash) {
                        _logger?.LogError( "Downloaded filehash does not match Sha1 hash from backblaze." );
                        return false;
                    } else {
                        _logger?.LogInformation( "File downloaded successfully." );
                        response.OutputPath.LastWriteTime = response.LastModified;
                    }
                } else {
                    _logger?.LogWarning( "Download completed. Unable to verify downloaded content." );
                }
            } else {
                throw new NotImplementedException( "FileName downloads not implemented yet." );
            }

            activity?.Stop( );
            return true;
        }

        #endregion Download


        #region ListFiles

        public async Task<List<B2FileResponse>> ListFileVersions(
            string startFileName = "",
            string startFileId = "",
            bool singleCall = false,
            int maxFileCount = -1,
            string prefix = ""
        ) {
            using Activity? activity = s_source.StartActivity( "ListFileVersions" )?.Start( );
            if (_b2Api == null) {
                activity?.Stop( );
                throw new ApplicationException( "Initialize before listing file versions." );
            }

            List<B2FileResponse> result = await _b2Api.ListFileVersions(
                                            startFileName,
                                            startFileId,
                                            maxFileCount,
                                            singleCall,
                                            prefix
                                        );

            activity?.Stop( );
            return result;
        }

        #endregion ListFiles
    }
}
