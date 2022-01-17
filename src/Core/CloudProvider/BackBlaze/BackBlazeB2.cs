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
        private static ILog? s_log;
        private static B2? s_b2Api;
        private static FileHash? s_fileHash;
        private static int s_maxErrors;

        public static TelemetryLogger? Logger { get; set; }

        public static void Initialize(
            B2Config config,
            TelemetryLogger? logger = null
        ) {
            Logger = logger;
            s_log = logger?.ILog;
            Initialize( config );
        }

        public static void Initialize( B2Config config ) {
            using Activity? activity = s_source.StartActivity( "Initialize" )?.Start( );

            s_maxErrors = (config.MaxConsecutiveErrors <= 0) ? 1 : config.MaxConsecutiveErrors; // Requires a minimum of 1.
            int uploadThreads = (config.UploadThreads <= 0) ? 1 : config.UploadThreads; // Requires a minimum of 1.
            s_fileHash = new FileHash( s_log );
            s_b2Api = new(
                config.ApplicationKeyId,
                config.ApplicationKey,
                s_maxErrors,
                uploadThreads,
                config.BucketName,
                config.BucketId,
                Logger
            );

            ThreadStatusManager.Init( s_log );

            activity?.Stop( );
        }

        #region Upload

        // Interface Method - Does not return FileId.
        public static void UploadFile( UploadB2File upload ) { _ = UploadFile( upload, s_b2Api ).Result; }
        public static async Task<string> UploadFile( string path ) => await UploadFile( new FileInfo( path ) );
        public static async Task<string> UploadFile( FileInfo path ) =>
            await UploadFile( new UploadB2File( path ), s_b2Api );

        public static async Task<string> UploadFile(
            FileInfo path,
            string originalFileName,
            string uploadFilePath,
            string sha512Hash
        ) => await UploadFile( new UploadB2File( path, originalFileName, uploadFilePath, sha512Hash ), s_b2Api );

        internal static async Task<string> UploadFile( UploadB2File upload, B2? b2Api ) {
            using Activity? activity = s_source.StartActivity( "UploadFile" )?.Start( );

            if (b2Api == null || s_fileHash == null) {
                activity?.Stop( );
                throw new InvalidOperationException( "Initialize before uploading." );
            }
            if (File.Exists( upload.FilePath.FullName ) == false) {
                activity?.Stop( );
                throw new InvalidOperationException( "Cannot upload a file that doesn't exist." );
            }

            if (string.IsNullOrWhiteSpace( upload.CompleteSha512Hash )) {
                upload.CompleteSha512Hash = await s_fileHash.GetSha512FileHash( upload.FilePath );
            }
            upload.CompleteSha1Hash = await s_fileHash.GetSha1FileHash( upload.FilePath );
            upload.MimeType = MimeType.GetMimeTypeByExtension( upload.FilePath );

            AuthProcessData authData = b2Api._authorizationData;
            int recSize = authData.RecommendedPartSize ?? 0;
            int minimumLargeFileSize = (recSize * 2) + authData.AbsoluteMinimumPartSize ?? 0;

            if (minimumLargeFileSize == 0) {
                activity?.Stop( );
                throw new InvalidOperationException( "Received an invalid response from BackBlaze." );
            }
            bool smallFileUpload = upload.FilePath.Length < minimumLargeFileSize;

            if (smallFileUpload) {
                s_log?.Debug( $"SmallFileUpload: " );
                s_log?.Debug( upload );

                s_log?.Debug( "Getting Small File Upload Url" );
                upload = await b2Api.NewSmallFileUploadUrl( upload );
                s_log?.Debug( upload );

                s_log?.Debug( "Uploading Small File" );
                upload = await b2Api.NewSmallFileUpload( upload );
            } else /* Large File Upload */ {
                s_log?.Debug( $"LargeFileUpload: " );
                s_log?.Debug( upload );

                int totalParts = (int)Math.Floor( Convert.ToDecimal( upload.FilePath.Length / recSize ) );
                int finalLength = (int)(upload.FilePath.Length - (totalParts * (long)recSize));
                totalParts++;

                if (finalLength < authData.AbsoluteMinimumPartSize) {
                    // Handle Edge Cases where remainder is smaller than minimum chunk size.
                    totalParts--;
                    finalLength += recSize;
                }

                int threadCount = totalParts < b2Api.ApplicationData.UploadThreads ?
                                    totalParts :
                                    b2Api.ApplicationData.UploadThreads;

                s_log?.Info( "Uploading Large File to Backblaze." );

                // Get FileId.
                s_log?.Debug( "Getting FileId For Large File." );
                upload = await b2Api.NewStartLargeFileURL( upload );
                s_log?.Debug( upload );

                s_log?.Info( "Uploading Large File Parts Async" );
                s_log?.Info( $"Splitting file into {totalParts - 1} {recSize} byte chunks and 1 {finalLength} chunk." );
                s_log?.Info( $"Chunks will be uploaded asyncronously via {threadCount} upload streams." );

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
                    s_log?.Fatal( $"filePartQueue part length total does not equal files length." );
                    throw new InvalidOperationException( "Failed to upload full file." );
                }

                List<Task<bool>> uploadTasks = new( );

                for (int thread = 0; thread < threadCount; thread++) {
                    s_log?.Debug( $"Thread#{thread} - Adding Task to Task List." );
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
                s_log?.Info( "Uploaded Large File Parts Async" );

                s_log?.Info( "Finishing Large File Upload." );
                if (resultsList != null) {
                    foreach (LargeFilePartReturn result in resultsList) {
                        upload.Sha1PartsList.Add( new( result.PartNumber, result.Sha1Hash ) );
                        upload.TotalBytesSent += result.DataSize;
                    }
                }


                s_log?.Debug( "Thread UploadStats:" );
                string stats = "{\n  \"ThreadStats\": [\n";
                foreach (UploadThreadStatistics stat in b2Api.ThreadStats) {
                    stats += $"{stat},\n";
                }
                stats = stats.TrimEnd( '\n' ).TrimEnd( ',' );
                stats += "\n  ]\n}";
                s_log?.Debug( stats );

                await b2Api.FinishUploadLargeFile( upload );

                ThreadStatusManager.Reset( );
                foreach (FailureInfo failure in b2Api.FailureDetails) {
                    failure.Reset( );
                }
            }
            activity?.Stop( );
            return upload.FileId;
        }

        private static async Task<bool> UploadLargeFileParts(
            UploadB2File upload,
            B2 b2Api,
            long partSize,
            ConcurrentBag<LargeFilePartReturn> resultsList,
            ConcurrentStack<FilePartInfo> filePartQueue,
            int thread
        ) {
            using Activity? activity = s_source.StartActivity( "UploadLargeFileParts" )?.Start( );
            if (s_fileHash == null) {
                activity?.Stop( );
                throw new InvalidOperationException( "Initialize before uploading." );
            }

            UploadB2File? threadUpload = await b2Api.NewUploadLargeFilePartUrl( upload );

            ThreadStatusManager.AddActiveThread( thread );

            int count = 1;
            while (filePartQueue.IsEmpty == false) {

                filePartQueue.TryPop( out FilePartInfo? partInfo );
                if (partInfo == null) { continue; }
                string pretxt = $"Thread#{thread} Part#{partInfo.PartNumber}";

                long start = partSize * (partInfo.PartNumber - 1);

                s_log?.Debug( $"{pretxt} - Retrieving Sha1 Hash for FileChunk" );
                if (string.IsNullOrWhiteSpace( partInfo.Sha1Hash )) {
                    partInfo.Sha1Hash = await s_fileHash.GetSHA1HashForFileChunkAsync( upload.FilePath, partInfo.Data, start );
                }

                bool success = false;
                try {
                    s_log?.Info( $"Thread#{thread} " +
                        $"Uploading LargeFile '{upload.OriginalFileName}' Part#{partInfo.PartNumber}." );
                    s_log?.Info( $"{pretxt} FileName      : {upload.FilePath.Name}" );
                    s_log?.Info( $"{pretxt} UploadFilePath: {upload.UploadFilePath}" );
                    s_log?.Info( $"{pretxt} PartSha1Hash  : {partInfo.Sha1Hash}" );
                    s_log?.Info( $"{pretxt} ContentSize   : {partInfo.Data.Length}" );

                    UploadB2FilePart uploadPart = new( threadUpload, partInfo.Sha1Hash, partInfo.PartNumber, partInfo.Data );

                    await b2Api.UploadLargeFilePart( uploadPart, thread );

                    //Upload segment of file data, Adds to TotalBytesSent +Sha1PartsList
                    s_log?.Info(
                        $"{pretxt}: LargeFile '{upload.OriginalFileName}' part uploaded successfully." +
                        $" Parts Sha1Hash: {uploadPart.PartSha1Hash}"
                    );
                    resultsList.Add( new( partInfo.PartNumber, uploadPart.PartSha1Hash, uploadPart.Content.Length ) );
                    s_log?.Debug(
                        $"Thread#{thread} Part#{partInfo.PartNumber} -  RESULTS LIST COUNT: " +
                        resultsList.Select( x => x.Sha1Hash ).Count( )
                    );
                    s_log?.Debug( $"ActiveThreadCount:   {ThreadStatusManager.ActiveThreadsCount}" );
                    s_log?.Debug( $"SleepingThreadCount: {ThreadStatusManager.SleepingThreadsCount}" );
                    success = true;
                } catch (HttpRequestException e) {
                    filePartQueue.Push( partInfo );
                    b2Api.HandleBackBlazeException( e, count, thread, filePartQueue );
                    count++;
                    threadUpload = await b2Api.NewUploadLargeFilePartUrl( upload );
                } catch (Exception ex) {
                    s_log?.Warn( $"Thread#{thread} had an exception", ex );
                    filePartQueue.Push( partInfo );
                    ThreadStatusManager.RemoveActiveThread( );
                    s_log?.Debug( $"There are #{ThreadStatusManager.ActiveThreadsCount} other active threads." );
                    activity?.Stop( );
                    return false;
                }

                if (count >= b2Api.ApplicationData.MaxErrors && success != true) {
                    s_log?.Error( $"Thread#{thread} hit max errors. Thread shutting down." );
                    filePartQueue.Push( partInfo );
                    activity?.Stop( );
                    ThreadStatusManager.RemoveActiveThread( );
                    return false;
                } else if (success) {
                    count = 1;
                }
            }

            s_log?.Info( $"Thread#{thread} Finished Assigned Work." );
            ThreadStatusManager.AddCompletedThread( thread );
            activity?.Stop( );
            return true;
        }

        private static void DetermineMultiPartUploadSuccessStatus(
            List<Task<bool>> tasks,
            ConcurrentStack<FilePartInfo> filePartQueue
        ) {
            using Activity? activity = s_source.StartActivity( "DetermineMultiPartUploadSuccessStatus" )?.Start( );

            bool success = true;
            List<bool> statusList = new( );
            foreach (Task<bool> task in tasks) {
                if (task.Exception != null || task.IsCanceled || task.IsCompletedSuccessfully != true) {
                    s_log?.Error( "Task was not successful." );
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
                        s_log?.Error( logMessage );
                    } else {
                        s_log?.Error( "There was no exception." );
                        s_log?.Error( $"task.IsCompletedSuccessfully: {task.IsCompletedSuccessfully}" );
                        s_log?.Error( $"task.IsCanceled: {task.IsCanceled}." );
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

        public static void DownloadFile( DownloadB2File download ) {
            using Activity? activity = s_source.StartActivity( "DownloadFile" )?.Start( );

            if (s_b2Api == null) {
                activity?.Stop( );
                throw new InvalidOperationException( "Initialize before downloading." );
            }
            _ = DownloadFile( download, s_b2Api ).Result;

            activity?.Stop( );
        }

        internal static async Task<bool> DownloadFile( DownloadB2File download, B2 b2Api ) {
            using Activity? activity = s_source.StartActivity( "DownloadFile.Async" )?.Start( );

            if (s_fileHash == null) {
                activity?.Stop( );
                throw new InvalidOperationException( "Initialize before downloading." );
            }

            if (string.IsNullOrWhiteSpace( download.FileId ) == false) {
                B2DownloadResponse response = await b2Api.DownloadFileID( download.FileId, download.OutputPath );
                s_log?.Debug( "Download Response:" + response );
                if (string.IsNullOrWhiteSpace( response.Sha1FileHash ) == false) {
                    string downloadedSha1Hash = s_fileHash.GetSha1FileHash( response.OutputPath.FullName );
                    if (response.Sha1FileHash != downloadedSha1Hash) {
                        s_log?.Error( "Downloaded filehash does not match Sha1 hash from backblaze." );
                        return false;
                    } else {
                        s_log?.Info( "File downloaded successfully." );
                        response.OutputPath.LastWriteTime = response.LastModified;
                    }
                } else {
                    s_log?.Warn( "Download completed. Unable to verify downloaded content." );
                }
            } else {
                throw new NotImplementedException( "FileName downloads not implemented yet." );
            }

            activity?.Stop( );
            return true;
        }

        #endregion Download


        #region ListFiles

        public static async Task<List<B2FileResponse>> ListFileVersions(
            string startFileName = "",
            string startFileId = "",
            bool singleCall = false,
            int maxFileCount = -1,
            string prefix = ""
        ) {
            using Activity? activity = s_source.StartActivity( "ListFileVersions" )?.Start( );
            if (s_b2Api == null) {
                activity?.Stop( );
                throw new InvalidOperationException( "Initialize before listing file versions." );
            }

            List<B2FileResponse> result = await s_b2Api.ListFileVersions(
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
