using System.Collections.Concurrent;
using System.Diagnostics;
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
        private readonly int _maxErrors;
        private readonly TelemetryLogger? _logger;

        public BackBlazeB2(
            B2Config config,
            TelemetryLogger? logger = null
        ) {
            _logger = logger;
            _log = logger?.ILog;
            _maxErrors = (config.MaxConsecutiveErrors <= 0) ? 1 : config.MaxConsecutiveErrors; // Requires a minimum of 1.
            int uploadThreads = (config.UploadThreads <= 0) ? 1 : config.UploadThreads; // Requires a minimum of 1.
            _fileHash = new FileHash( _log );
            _b2Api = new(
                config.ApplicationKeyId,
                config.ApplicationKey,
                _maxErrors,
                uploadThreads,
                config.BucketName,
                config.BucketId,
                _logger
            );
        }

        #region Upload

        // Interface Method - Does not return FileId.
        public void UploadFile( UploadB2File upload ) { _ = UploadFile( upload, _b2Api ).Result; }
        public async Task<string> UploadFile( string path ) => await UploadFile( new FileInfo( path ) );
        public async Task<string> UploadFile( FileInfo path ) =>
            await UploadFile( new UploadB2File( path ), _b2Api );

        public async Task<string> UploadFile(
            FileInfo path,
            string originalFileName,
            string uploadFilePath,
            string sha512Hash
        ) => await UploadFile( new UploadB2File( path, originalFileName, uploadFilePath, sha512Hash ), _b2Api );

        internal async Task<string> UploadFile( UploadB2File upload, B2? b2Api ) {
            using Activity? activity = s_source.StartActivity( "UploadFile" )?.Start( );

            if (b2Api == null || _fileHash == null) {
                activity?.Stop( );
                throw new InvalidOperationException( "Initialize before uploading." );
            }
            if (File.Exists( upload.FilePath.FullName ) == false) {
                activity?.Stop( );
                throw new InvalidOperationException( "Cannot upload a file that doesn't exist." );
            }

            if (string.IsNullOrWhiteSpace( upload.CompleteSha512Hash )) {
                upload.CompleteSha512Hash = await _fileHash.GetSha512FileHash( upload.FilePath );
            }
            upload.CompleteSha1Hash = await _fileHash.GetSha1FileHash( upload.FilePath );
            upload.MimeType = MimeType.GetMimeTypeByExtension( upload.FilePath );

            int recSize = b2Api.RecommendedPartSize ?? 0;
            int minimumLargeFileSize = (recSize * 2) + b2Api.AbsoluteMinimumPartSize ?? 0;

            if (minimumLargeFileSize == 0) {
                activity?.Stop( );
                throw new InvalidOperationException( "Received an invalid response from BackBlaze." );
            }
            bool smallFileUpload = upload.FilePath.Length < minimumLargeFileSize;

            if (smallFileUpload) {
                _log?.Debug( $"SmallFileUpload: " );
                _log?.Debug( upload );

                _log?.Debug( "Getting Small File Upload Url" );
                upload = await b2Api.NewSmallFileUploadUrl( upload );
                _log?.Debug( upload );

                _log?.Debug( "Uploading Small File" );
                upload = await b2Api.NewSmallFileUpload( upload );
            } else /* Large File Upload */ {
                _log?.Debug( $"LargeFileUpload: " );
                _log?.Debug( upload );

                int totalParts = (int)Math.Floor( Convert.ToDecimal( upload.FilePath.Length / recSize ) );
                int finalLength = (int)(upload.FilePath.Length - (totalParts * (long)recSize));
                totalParts++;

                if (finalLength < (b2Api.AbsoluteMinimumPartSize ?? 0)) {
                    // Handle Edge Cases where remainder is smaller than minimum chunk size.
                    totalParts--;
                    finalLength += recSize;
                }

                int threadCount = totalParts < b2Api.ThreadManager.ActiveThreadCount ?
                                    totalParts :
                                    b2Api.ThreadManager.ActiveThreadCount;

                _log?.Info( "Uploading Large File to Backblaze." );

                // Get FileId.
                _log?.Debug( "Getting FileId For Large File." );
                upload = await b2Api.NewStartLargeFileURL( upload );
                _log?.Debug( upload );

                _log?.Info( "Uploading Large File Parts Async" );
                _log?.Info( $"Splitting file into {totalParts - 1} {recSize} byte chunks and 1 {finalLength} chunk." );
                _log?.Info( $"Chunks will be uploaded asyncronously via {threadCount} upload streams." );

                ConcurrentBag<LargeFilePartReturn>? resultsList = new( );
                ConcurrentStack<FilePartInfo> filePartQueue = new( );
                long lengthTotal = 0;
                // Populate the queue.
                for (int i = 1; i <= totalParts; i++) {
                    int partLength = i == totalParts ? finalLength : recSize;
                    filePartQueue.Push( new FilePartInfo( i, partLength ) );
                    lengthTotal += partLength;
                }
                if (lengthTotal != upload.FilePath.Length) {
                    _log?.Fatal( $"filePartQueue part length total does not equal files length." );
                    throw new InvalidOperationException( "Failed to upload full file." );
                }

                List<Task<bool>> uploadTasks = new( );

                for (int thread = 0; thread < threadCount; thread++) {
                    _log?.Debug( $"Thread#{thread} - Adding Task to Task List." );
                    uploadTasks.Add(
                        UploadLargeFileParts(
                            upload,
                            b2Api,
                            recSize,
                            resultsList,
                            filePartQueue,
                            thread
                        )
                    );
                    Thread.Sleep( 100 );
                }

                while (uploadTasks.Any( x => x.IsCompleted == false )) { Thread.Sleep( 1000 ); }
                DetermineMultiPartUploadSuccessStatus( uploadTasks, filePartQueue );
                _log?.Info( "Uploaded Large File Parts Async" );

                _log?.Info( "Finishing Large File Upload." );
                if (resultsList != null) {
                    foreach (LargeFilePartReturn result in resultsList) {
                        upload.Sha1PartsList.Add( new( result.PartNumber, result.Sha1Hash ) );
                        upload.TotalBytesSent += result.DataSize;
                    }
                }

                _log?.Debug( "Thread UploadStats:" );
                b2Api.ThreadManager.ShowThreadStatistics( true );

                await b2Api.FinishUploadLargeFile( upload );

                foreach (FailureInfo failure in b2Api.ThreadManager.FailureDetails) {
                    failure.Reset( );
                }
            }
            activity?.Stop( );
            return upload.FileId;
        }

        private async Task<bool> UploadLargeFileParts(
            UploadB2File upload,
            B2 b2Api,
            long partSize,
            ConcurrentBag<LargeFilePartReturn> resultsList,
            ConcurrentStack<FilePartInfo> filePartQueue,
            int thread
        ) {
            using Activity? activity = s_source.StartActivity( "UploadLargeFileParts" )?.Start( );
            if (_fileHash == null) {
                activity?.Stop( );
                throw new InvalidOperationException( "Initialize before uploading." );
            }

            UploadB2File? threadUpload = await b2Api.NewUploadLargeFilePartUrl( upload );

            int count = 1;
            while (filePartQueue.IsEmpty == false) {

                filePartQueue.TryPop( out FilePartInfo? partInfo );
                if (partInfo == null) { continue; }
                string pretxt = $"Thread#{thread} Part#{partInfo.PartNumber}";

                long start = partSize * (partInfo.PartNumber - 1);

                _log?.Debug( $"{pretxt} - Retrieving Sha1 Hash for FileChunk" );
                if (string.IsNullOrWhiteSpace( partInfo.Sha1Hash )) {
                    partInfo.Sha1Hash = await _fileHash.GetSHA1HashForFileChunkAsync( upload.FilePath, partInfo.Data, start );
                }

                bool success = false;
                try {
                    _log?.Info( $"Thread#{thread} " +
                        $"Uploading LargeFile '{upload.OriginalFileName}' Part#{partInfo.PartNumber}." );
                    _log?.Info( $"{pretxt} FileName      : {upload.FilePath.Name}" );
                    _log?.Info( $"{pretxt} UploadFilePath: {upload.UploadFilePath}" );
                    _log?.Info( $"{pretxt} PartSha1Hash  : {partInfo.Sha1Hash}" );
                    _log?.Info( $"{pretxt} ContentSize   : {partInfo.Data.Length}" );

                    UploadB2FilePart uploadPart = new( threadUpload, partInfo.Sha1Hash, partInfo.PartNumber, partInfo.Data );

                    await b2Api.UploadLargeFilePart( uploadPart, thread );

                    //Upload segment of file data, Adds to TotalBytesSent +Sha1PartsList
                    _log?.Info(
                        $"{pretxt}: LargeFile '{upload.OriginalFileName}' part uploaded successfully." +
                        $" Parts Sha1Hash: {uploadPart.PartSha1Hash}"
                    );
                    resultsList.Add( new( partInfo.PartNumber, uploadPart.PartSha1Hash, uploadPart.Content.Length ) );
                    _log?.Debug(
                        $"Thread#{thread} Part#{partInfo.PartNumber} -  RESULTS LIST COUNT: " +
                        resultsList.Select( x => x.Sha1Hash ).Count( )
                    );
                    success = true;
                } catch (HttpRequestException e) {
                    filePartQueue.Push( partInfo );
                    b2Api.HandleBackBlazeException( e, count, thread, filePartQueue );
                    count++;
                    threadUpload = await b2Api.NewUploadLargeFilePartUrl( upload );
                } catch (Exception ex) {
                    _log?.Warn( $"Thread#{thread} had an exception", ex );
                    filePartQueue.Push( partInfo );
                    activity?.Stop( );
                    return false;
                }

                if (count >= _maxErrors && success != true) {
                    _log?.Error( $"Thread#{thread} hit max errors. Thread shutting down." );
                    filePartQueue.Push( partInfo );
                    activity?.Stop( );
                    return false;
                } else if (success) {
                    count = 1;
                }
            }

            _log?.Info( $"Thread#{thread} Finished Assigned Work." );
            activity?.Stop( );
            return true;
        }

        private void DetermineMultiPartUploadSuccessStatus(
            List<Task<bool>> tasks,
            ConcurrentStack<FilePartInfo> filePartQueue
        ) {
            using Activity? activity = s_source.StartActivity( "DetermineMultiPartUploadSuccessStatus" )?.Start( );

            bool success = true;
            List<bool> statusList = new( );
            foreach (Task<bool> task in tasks) {
                if (task.Exception != null || task.IsCanceled || task.IsCompletedSuccessfully != true) {
                    _log?.Error( "Task was not successful." );
                    success = false;

                    if (task.Exception != null) {
                        AggregateException ex = task.Exception;
                        string logMessage = ex.Message;
                        foreach (Exception exception in ex.InnerExceptions) {
                            Type expType = exception.GetType( );
                            string[] myKeys = new string[exception.Data.Count];
                            exception.Data.Keys.CopyTo( myKeys, 0 );

                            logMessage += $"\nExceptionType : {expType}";
                            logMessage += $"\nMessage       : {exception.Message}";
                            logMessage += $"\nData          : {exception.Data}";
                            if (myKeys.Length > 0) { logMessage += "\nData          : "; }
                            for (int i = 0; i < exception.Data.Count; i++) {
                                logMessage += $"\n{i,-5}. '{myKeys[i]}' '{exception.Data[myKeys[i]]}'";
                            }
                            logMessage += $"\nHelpLink      : {exception.HelpLink}";
                            logMessage += $"\nSource        : {exception.Source}";
                            logMessage += $"\nTargetSite    : {exception.TargetSite}";
                            logMessage += $"\nHResult       : {exception.HResult}";
                            logMessage += $"\nStackTrace    : {exception.StackTrace}";
                            logMessage += $"\nInnerException: {exception.InnerException}\n";
                        }
                        _log?.Error( logMessage );
                    } else {
                        _log?.Error( "There was no exception." );
                        _log?.Error( $"task.IsCompletedSuccessfully: {task.IsCompletedSuccessfully}" );
                        _log?.Error( $"task.IsCanceled: {task.IsCanceled}." );
                    }
                } else {
                    statusList.Add( task.Result );
                }
            }

            if (statusList.Contains( true ) == false || success == false || filePartQueue.IsEmpty != true) {
                throw new InvalidOperationException( "Multi-Part upload was unsuccessful." );
            }

            activity?.Stop( );
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
