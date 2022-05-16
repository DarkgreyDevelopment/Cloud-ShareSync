using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Cloud_ShareSync.Core.CloudProvider.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2 {
    public partial class B2Api {

        public async Task<string> UploadFile( UploadFileInfo upload ) {
            using Activity? activity = _source.StartActivity( "UploadFile" )?.Start( );

            if (File.Exists( upload.FilePath.FullName ) == false) {
                activity?.Stop( );
                throw new ApplicationException( "Cannot upload a file that doesn't exist." );
            }
            _log?.LogInformation( "Uploading \"{string}\" to backblaze.", upload.FilePath.FullName );
            _log?.LogInformation( "Uploading as: \"{string}\"", upload.UploadFilePath );

            return (upload.FilePath.Length < LargeFileSize) ?
               await UploadSmallFile( upload ) :
               await UploadLargeFile( upload );
        }

        private async Task<string> UploadSmallFile( UploadFileInfo upload ) {
            int count = 0;
            RestartUploadFileException? ex = null;
            TimeSpan[] timeSpans = B2RequestHandler.GetJitterBackOffTimeSpans( _initData.MaxErrors );
            string? fileId = null;
            do {
                count++;
                try {
                    _log?.LogInformation( "Getting Small File Upload Url" );
                    GetUploadUrl uploadUrl = await GetUploadUrl.CallApi(
                        AuthToken,
                        _initData.BucketId,
                        GetHttpClient( ),
                        _initData.MaxErrors,
                        _log
                    );

                    _log?.LogInformation( "Uploading Small File to Backblaze" );
                    UploadFile uploadFile = await V2Api.Endpoints.UploadFile.CallApi(
                        uploadUrl,
                        upload,
                        GetHttpClient( ),
                        count,
                        timeSpans,
                        _log
                    );
                    fileId =
                        uploadFile.contentSha1 == upload.SHA1 &&
                        uploadFile.contentLength == upload.FilePath.Length ?
                            uploadFile.fileId : null;
                } catch (RestartUploadFileException e) {
                    ex = e;
                    continue;
                } catch (NewAuthTokenRequiredException) {
                    await UpdateAuthData( );
                }
            } while (fileId == null && count < _initData.MaxErrors);

            if (fileId == null) {
                string expMsg = $"Small file upload failed. Failed to upload '{upload.UploadFilePath}'.";
                throw ex == null ?
                    new FailedB2RequestException( expMsg ) :
                    new FailedB2RequestException( expMsg, ex );
            } else {
                return fileId;
            }

        }

        private async Task<string> UploadLargeFile( UploadFileInfo upload ) {
            // Step 1 - StartLargeFile (Get FileId)
            StartLargeFile start = await GetFileId( upload );

            // Step 2 - b2_get_upload_part_url (for each thread uploading)
            ConcurrentBag<FilePartResult> filePartsResults = new( );
            ThreadArbiter arbiter = ArbitrateThreads( upload.FilePath.Length );
            ConcurrentStack<UploadResultInfo> filePartStack = CreatePartsStack( arbiter );
            List<GetUploadPartUrl> urlList = await GetUploadPartUrls( arbiter.ThreadCount, start.fileId! );

            // Step 3 - b2_upload_part OR b2_copy_part for each part of the file
            List<Task> tasks = await AwaitUploadParts( upload, filePartStack, filePartsResults, urlList );
            DetermineMultiPartUploadSuccessStatus( tasks, filePartStack );

            // Step 4 - b2_finish_large_file
            FinishLargeFile result = await FinishLargeFileUpload(
                start.fileId!,
                filePartsResults
            );

            return result.fileId!;
        }

        private async Task<StartLargeFile> GetFileId( UploadFileInfo upload ) {
            _log?.LogDebug( "Getting FileId For Large File." );
            StartLargeFile result;
            try {
                result = await StartLargeFile.CallApi(
                    AuthToken,
                    _initData.BucketId,
                    upload,
                    GetHttpClient( ),
                    _initData.MaxErrors,
                    _log
                );
            } catch (NewAuthTokenRequiredException) {
                await UpdateAuthData( );
                result = await StartLargeFile.CallApi(
                    AuthToken,
                    _initData.BucketId,
                    upload,
                    GetHttpClient( ),
                    _initData.MaxErrors,
                    _log
                );
            }
            return result;
        }

        private ThreadArbiter ArbitrateThreads( long contentLength ) {
            ThreadArbiter arbiter = new(
                contentLength,
                AuthToken.recommendedPartSize,
                AuthToken.absoluteMinimumPartSize,
                _initData.HttpThreads,
                _log
            );
            _log?.LogInformation(
                "Splitting file into {int} {int} byte chunks and 1 {int} chunk.",
                arbiter.TotalParts - 1,
                arbiter.PartSize,
                arbiter.FinalSize
            );
            return arbiter;
        }

        private ConcurrentStack<UploadResultInfo> CreatePartsStack( ThreadArbiter arbiter ) {
            ConcurrentStack<UploadResultInfo> filePartStack = new( );
            for (int i = arbiter.TotalParts; i >= 1; i--) {
                int partLength = i == arbiter.TotalParts ? arbiter.FinalSize : arbiter.PartSize;
                filePartStack.Push(
                    new( new( i, partLength, (long)(i - 1) * arbiter.PartSize ) )
                );
            }
            return filePartStack;
        }

        private async Task<List<GetUploadPartUrl>> GetUploadPartUrls(
            int urlCount,
            string fileId
        ) {
            List<GetUploadPartUrl> urlList = new( );
            for (int i = 0; i < urlCount; i++) {
                GetUploadPartUrl parturl;
                try {
                    parturl = await GetUploadPartUrl.CallApi(
                        AuthToken,
                        fileId,
                        GetHttpClient( ),
                        _initData.MaxErrors,
                        _log
                    );
                } catch (NewAuthTokenRequiredException) {
                    await UpdateAuthData( );
                    parturl = await GetUploadPartUrl.CallApi(
                        AuthToken,
                        fileId,
                        GetHttpClient( ),
                        _initData.MaxErrors,
                        _log
                    );
                }
                urlList.Add( parturl );
            }
            _log?.LogInformation( "Created {int} upload urls.", urlList.Count );
            return urlList;
        }

        private async Task<List<Task>> AwaitUploadParts(
            UploadFileInfo upload,
            ConcurrentStack<UploadResultInfo> filePartStack,
            ConcurrentBag<FilePartResult> filePartsResults,
            List<GetUploadPartUrl> urlList
        ) {
            _log?.LogInformation( "Uploading Large File to Backblaze." );
            List<Task> tasks = new( );
            int count = 1;
            foreach (GetUploadPartUrl url in urlList) {
                tasks.Add(
                    UploadLargeFileParts(
                        upload,
                        url,
                        filePartStack,
                        filePartsResults,
                        count
                    )
                );
                count++;
            }
            while (tasks.Any( x => x.IsCompleted == false )) {
                await Task.Delay( 1000 );
            }
            return tasks;
        }

        private async Task UploadLargeFileParts(
            UploadFileInfo upload,
            GetUploadPartUrl urlData,
            ConcurrentStack<UploadResultInfo> filePartStack,
            ConcurrentBag<FilePartResult> filePartsResults,
            int thread
        ) {
            while (filePartStack.IsEmpty == false) {
                bool poppedItem = filePartStack.TryPop( out UploadResultInfo? partInfo );
                if (poppedItem == false || partInfo == null) { continue; }

                FilePartResult result = partInfo.Result;
                FilePartInfo? partData = partInfo.Info;

                string pretxt = $"Thread#{thread} Part#{result.PartNumber}";

                if (partData == null) {
                    _log?.LogDebug( "{string} - Retrieving Sha1 Hash for FileChunk", pretxt );
                    partData = new(
                        upload.FilePath,
                        result.PartNumber,
                        result.PartSize,
                        result.Offset,
                        _log
                    );
                }
                result.PartSha1 = partData.SHA1;

                UploadResultInfo stackReturnInfo = new( result ) { Info = partData };
                try {
                    FilePartResult data = await UploadLargeFilePart(
                        GetHttpClient( ),
                        urlData,
                        upload,
                        partData,
                        result,
                        pretxt,
                        _initData.MaxErrors,
                        _log
                    );
                    filePartsResults.Add( data );
                } catch (Exception ex) {
                    _log?.LogError(
                        "{string} - File part failed to upload.\n{string}",
                        pretxt,
                        BuildExceptionMessage( ex )
                    );
                    filePartStack.Push( stackReturnInfo );
                    throw;
                }
            }
        }

        internal async Task<FilePartResult> UploadLargeFilePart(
            HttpClient client,
            GetUploadPartUrl urlData,
            UploadFileInfo upload,
            FilePartInfo partData,
            FilePartResult result,
            string pretxt,
            int maxErrors,
            ILogger? log
        ) {
            UploadPart? uploadResult = null;
            int count = 0;
            RestartUploadPartException? ex;
            TimeSpan[] timeSpans = B2RequestHandler.GetJitterBackOffTimeSpans( maxErrors );
            do {
                count++;
                try {
                    log?.LogDebug( "{string} - Uploading file part.", pretxt );
                    uploadResult = await UploadPart.CallApi(
                        urlData,
                        upload,
                        partData,
                        client,
                        count,
                        timeSpans,
                        log
                    );
                    return (
                        uploadResult.contentSha1 == result.PartSha1 &&
                        uploadResult.contentLength == result.PartSize
                    ) ? result :
                        throw new FailedB2RequestException(
                            "Part upload failed. Response metadata indicates uploaded content doesn't match content on disk."
                        );
                } catch (RestartUploadPartException e) {
                    ex = e;
                    log?.LogWarning(
                        "An error has occurred while uploading a file part to BackBlaze. {string}",
                        e.Message
                    );
                    try {
                        urlData = await GetUploadPartUrl.CallApi(
                            AuthToken,
                            urlData.fileId,
                            client,
                            maxErrors,
                            log
                        );
                    } catch (NewAuthTokenRequiredException) {
                        await UpdateAuthData( );
                        urlData = await GetUploadPartUrl.CallApi(
                            AuthToken,
                            urlData.fileId,
                            client,
                            maxErrors,
                            log
                        );
                    }
                    continue;
                }
            } while (uploadResult == null && count < maxErrors);

            throw new FailedB2RequestException(
                $"Part upload failed. Upload file part aborting after {count} attempts.",
                ex
            );
        }

        private async Task<FinishLargeFile> FinishLargeFileUpload(
            string fileId,
            ConcurrentBag<FilePartResult> filePartsResults
        ) {
            B2FinishLargeFileRequest finishLargeFileData = CreateFinishLargeFileRequest(
                fileId,
                filePartsResults
            );
            _log?.LogDebug( "Finishing Large File Upload.\n{string}", finishLargeFileData.ToString( ) );
            FinishLargeFile finishLargeFile;
            try {
                finishLargeFile = await FinishLargeFile.CallApi(
                    AuthToken,
                    finishLargeFileData,
                    GetHttpClient( ),
                    _initData.MaxErrors,
                    _log
                );
            } catch (NewAuthTokenRequiredException) {
                await UpdateAuthData( );
                finishLargeFile = await FinishLargeFile.CallApi(
                    AuthToken,
                    finishLargeFileData,
                    GetHttpClient( ),
                    _initData.MaxErrors,
                    _log
                );
            }
            return finishLargeFile;
        }

        private static B2FinishLargeFileRequest CreateFinishLargeFileRequest(
            string fileId,
            ConcurrentBag<FilePartResult> filePartsResults
        ) {
            List<KeyValuePair<int, string>> sha1PartsList = new( );
            while (filePartsResults.IsEmpty == false) {
                bool dequeue = filePartsResults.TryTake( out FilePartResult? partResult );
                if (dequeue) { sha1PartsList.Add( partResult!.NewPartKVP( ) ); }
            }
            return new( fileId, sha1PartsList );
        }

        private void DetermineMultiPartUploadSuccessStatus(
            List<Task> tasks,
            ConcurrentStack<UploadResultInfo> filePartStack
        ) {
            using Activity? activity = _source.StartActivity( "DetermineMultiPartUploadSuccessStatus" )?.Start( );

            LogTaskStatus( tasks );
            if (filePartStack.IsEmpty != true) {
                throw new ApplicationException( "Multi-Part upload was unsuccessful." );
            }

            activity?.Stop( );
        }

        private void LogTaskStatus( List<Task> tasks ) {
            int count = 0;
            foreach (Task task in tasks) {
                if (TaskStatusFailed( task )) {
                    string logMessage = $"Upload task{count} was not successful. Task status: " +
                        $"IsCompletedSuccessfully? {task.IsCompletedSuccessfully}" +
                        $"IsCanceled? {task.IsCanceled}.";
                    if (task.Exception != null) {
                        logMessage = $"Upload task{count} encountered errors:\n{task.Exception.Message}";
                        foreach (Exception exception in task.Exception.InnerExceptions) {
                            logMessage += BuildExceptionMessage( exception );
                        }
                    }
                    _log?.LogError( "{string}", logMessage );
                }
                count++;
            }
        }

        private static bool TaskStatusFailed( Task task ) =>
            task.Exception != null || task.IsCanceled || task.IsCompletedSuccessfully != true;

        private static string BuildExceptionMessage( Exception exception ) {
            Type expType = exception.GetType( );

            string logMessage = exception.Message;

            logMessage += $"ExceptionType : {expType}\n";
            logMessage += $"Message       : {exception.Message}\n";
            logMessage += WriteExceptionExtraData( exception );
            logMessage += $"HelpLink      : {exception.HelpLink}\n";
            logMessage += $"Source        : {exception.Source}\n";
            logMessage += $"TargetSite    : {exception.TargetSite}\n";
            logMessage += $"HResult       : {exception.HResult}\n";
            logMessage += $"StackTrace    : {exception.StackTrace}\n";
            logMessage += $"InnerException: {exception.InnerException}\n\n";
            return logMessage;
        }

        private static string WriteExceptionExtraData( Exception exception ) {
            string logMessage = "";
            if (exception.Data.Count > 0) {
                logMessage += "Data          :\n";
                foreach (DictionaryEntry de in exception.Data) {
                    logMessage += $"              : Key: '{de.Key,-20}'      Value: {de.Value}\n";
                }
            }
            return logMessage;
        }

    }
}
