﻿using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.CloudProvider.Interface;
using Cloud_ShareSync.Core.Configuration.Types.Cloud;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Logging;
using log4net;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {
    public class BackBlazeB2 : ICloudProvider {

        private static readonly ActivitySource s_source = new( "BackBlazeB2.PublicInterface" );
        private readonly ILog? _log;
        private readonly B2? _b2Api;
        private readonly FileHash? _fileHash;
        private readonly TelemetryLogger? _logger;
        private readonly int _maxErrors;

        public BackBlazeB2(
            B2Config config,
            TelemetryLogger? logger = null
        ) {
            _logger = logger;
            _log = logger?.ILog;
            _fileHash = new FileHash( _log );
            _maxErrors = (config.MaxConsecutiveErrors <= 0) ? 1 : config.MaxConsecutiveErrors; // Requires a minimum of 1.
            _b2Api = new(
                config.ApplicationKeyId,
                config.ApplicationKey,
                _maxErrors,
                (config.UploadThreads <= 0) ? 1 : config.UploadThreads, // Requires a minimum of 1.
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
                throw new InvalidOperationException( "Initialize before uploading." );
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
                        _log?.Fatal( "Failed to upload file to backblaze.", ex );
                        throw;
                    } else {
                        _log?.Error( "Error while uploading file to backblaze.", ex );
                    }
                    count++;

                    if (count < _maxErrors) {
                        _log?.Error( "Sleeping for a minute before retrying." );
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
                throw new InvalidOperationException( "Initialize before downloading." );
            }
            _ = DownloadFile( download, _b2Api ).Result;

            activity?.Stop( );
        }

        internal async Task<bool> DownloadFile( DownloadB2File download, B2 b2Api ) {
            using Activity? activity = s_source.StartActivity( "DownloadFile.Async" )?.Start( );

            if (_fileHash == null) {
                activity?.Stop( );
                throw new InvalidOperationException( "Initialize before downloading." );
            }

            if (string.IsNullOrWhiteSpace( download.FileId ) == false) {
                B2DownloadResponse response = await b2Api.DownloadFileID( download.FileId, download.OutputPath );
                _log?.Debug( "Download Response:" + response );
                if (string.IsNullOrWhiteSpace( response.Sha1FileHash ) == false) {
                    string downloadedSha1Hash = _fileHash.GetSha1FileHash( response.OutputPath.FullName );
                    if (response.Sha1FileHash != downloadedSha1Hash) {
                        _log?.Error( "Downloaded filehash does not match Sha1 hash from backblaze." );
                        return false;
                    } else {
                        _log?.Info( "File downloaded successfully." );
                        response.OutputPath.LastWriteTime = response.LastModified;
                    }
                } else {
                    _log?.Warn( "Download completed. Unable to verify downloaded content." );
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
                throw new InvalidOperationException( "Initialize before listing file versions." );
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
