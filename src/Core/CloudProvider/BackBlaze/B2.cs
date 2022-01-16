using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.HttpClientHandling;
using Cloud_ShareSync.Core.Logging;
using log4net;
using Microsoft.Extensions.DependencyInjection;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal class B2 {

        #region Fields

        private readonly ActivitySource _source = new( "B2" );
        private readonly ILog? _log;

        // Configuration vars
        internal const string AuthorizationURI = "https://api.backblazeb2.com/b2api/v2/b2_authorize_account";
        internal const string UserAgent = "Cloud-ShareSync_BackBlazeB2/0.0.1+dotnet/6.0";

        // Set by Ctor.
        internal readonly UploadThreadStatistics[] ThreadStats;
        internal readonly InitializationData ApplicationData;
        internal BackBlazeHttpServices _httpClients;
        private readonly List<Regex> _regexPatterns;

        internal AuthProcessData _authorizationData; // Set by valid authorization process
        internal FailureInfo[] _failureDetails; // Set upon error

        private readonly Random _random = new( );
        private readonly int[] _retryStartSec = new int[] { 0, 1, 3, 7 };

        #endregion Fields

        internal B2(
            string applicationKeyId,
            string applicationKey,
            int maxErrors,
            int uploadThreads,
            string bucketName,
            string bucketId,
            TelemetryLogger? logger = null
        ) {
            _log = logger?.ILog;
            _authorizationData = new( );
            ApplicationData = new(
                applicationKeyId,
                applicationKey,
                maxErrors,
                uploadThreads,
                bucketName,
                bucketId
            );

            List<UploadThreadStatistics> threadStats = new( );
            List<FailureInfo> failureDetails = new( );
            for (int i = 0; i < uploadThreads; i++) {
                threadStats.Add( new( i ) );
                failureDetails.Add( new( _retryStartSec[_random.Next( 0, _retryStartSec.Length )] ) );
            }
            _failureDetails = failureDetails.ToArray( );
            ThreadStats = threadStats.ToArray( );

            _httpClients = new BackBlazeHttpServices( uploadThreads, logger );
            // Get Auth Client / Get initial auth data.
            _authorizationData = GetBackBlazeGeneralClient( )
                                .NewAuthReturn( ApplicationData.Credentials )
                                .Result
                                .AuthData;

            _regexPatterns = new( );
            _regexPatterns.Add(
                new( "A connection attempt failed because the connected party did not properly respond", RegexOptions.Compiled )
            );
            _regexPatterns.Add( new( "^Error while copying content to a stream.$", RegexOptions.Compiled ) );
        }


        // HTTP Clients
        internal static BackBlazeHttpClient GetBackBlazeGeneralClient( ) =>
            BackBlazeHttpServices.Services.GetRequiredService<BackBlazeHttpClient>( );

        #region Authorization

        internal async Task<string> NewAuthToken( ) {
            using Activity? activity = _source.StartActivity( "NewAuthToken" )?.Start( );

            AuthReturn? authReturn = await GetBackBlazeGeneralClient( ).NewAuthReturn( ApplicationData.Credentials );
            _authorizationData = authReturn.AuthData;

            activity?.Stop( );
            return authReturn.AuthToken ??
                throw new InvalidB2Response(
                    AuthorizationURI,
                    new NullReferenceException( "AuthorizationToken" )
                );
        }

        #endregion Authorization


        #region Uploads

        #region SmallFileUploads

        internal async Task<UploadB2File> NewSmallFileUploadUrl( UploadB2File uploadObject ) {
            using Activity? activity = _source.StartActivity( "NewSmallFileUploadUrl" )?.Start( );

            string? uploadUri = _authorizationData.ApiUrl + "/b2api/v2/b2_get_upload_url";
            DateTimeOffset dto = new( uploadObject.FilePath.LastWriteTimeUtc );
            byte[] data = Encoding.UTF8.GetBytes( $"{{\"bucketId\":\"{ApplicationData.BucketId}\"}}" );

            JsonElement root = await GetBackBlazeGeneralClient( ).GetJsonResponse(
                uploadUri,
                HttpMethod.Post,
                await NewAuthToken( ),
                data,
                null
            );

            _log?.Debug( $"NewSmallFileUploadUrl Response: {root}" );

            uploadObject.UploadUrl =
                root.GetProperty( "uploadUrl" ).GetString( ) ??
                throw new InvalidB2Response(
                    uploadUri,
                    new NullReferenceException( "UploadUrl" )
                );

            uploadObject.AuthorizationToken =
                root.GetProperty( "authorizationToken" ).GetString( ) ??
                throw new InvalidB2Response(
                    uploadUri,
                    new NullReferenceException( "AuthorizationToken" )
                );

            activity?.Stop( );
            return uploadObject;
        }

        public async Task<UploadB2File> NewSmallFileUpload( UploadB2File uploadObject ) {
            using Activity? activity = _source.StartActivity( "NewSmallFileUpload" )?.Start( );

            string dto = new DateTimeOffset( uploadObject.FilePath.LastWriteTimeUtc )
                            .ToUnixTimeMilliseconds( )
                            .ToString( );

            byte[] data = File.ReadAllBytes( uploadObject.FilePath.FullName );

            /* Need to move this stuff to the client/ClientUtilities like the rest of the process. */
            /* Theres a bug that breaks things currently though so thats why this is separated. */
            // Create Initial Request
            HttpRequestMessage request = new( HttpMethod.Post, uploadObject.UploadUrl );
            // Add Authorization Headers
            request.Headers.TryAddWithoutValidation( "Authorization", uploadObject.AuthorizationToken );
            // Add UserAgent Headers
            request.Headers.TryAddWithoutValidation( "UserAgent", UserAgent );
            // Create request content
            request.Content = new ByteArrayContent( data );
            // Set request Content-Type
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( uploadObject.MimeType );
            // Set request Content-Length
            request.Content.Headers.ContentLength = data.Length;

            // Add additional content headers.
            request.Content.Headers.Add( "X-Bz-File-Name", CleanUploadPath( uploadObject.UploadFilePath, false ) );
            request.Content.Headers.Add( "X-Bz-Content-Sha1", uploadObject.CompleteSha1Hash );
            request.Content.Headers.Add( "X-Bz-Info-Author", "Cloud-ShareSync" );
            request.Content.Headers.Add( "X-Bz-Server-Side-Encryption", "AES256" );
            request.Content.Headers.Add( "X-Bz-Info-src_last_modified_millis", dto );
            request.Content.Headers.Add( "X-Bz-Info-sha512_filehash", uploadObject.CompleteSha512Hash );

            _log?.Debug( "NewSmallFileUpload Request: " + request.ToString( ) );

            JsonElement root = await GetBackBlazeGeneralClient( ).GetJsonResponse( request );

            // Set UploadObject values.
            uploadObject.FileId = root.GetProperty( "fileId" ).GetString( ) ??
                throw new InvalidB2Response(
                    uploadObject.UploadUrl,
                    new NullReferenceException( "fileId" )
                );
            uploadObject.TotalBytesSent = data.Length;
            uploadObject.Sha1PartsList.Add( new( 0, uploadObject.CompleteSha1Hash ) );

            activity?.Stop( );
            return uploadObject;
        }

        #endregion SmallFileUploads

        #region LargeFileUploads

        internal async Task<UploadB2File> NewStartLargeFileURL( UploadB2File uploadObject ) {
            using Activity? activity = _source.StartActivity( "NewStartLargeFileURL" )?.Start( );

            string uploadUri = _authorizationData.ApiUrl + "/b2api/v2/b2_start_large_file";
            DateTimeOffset dto = new( uploadObject.FilePath.LastWriteTimeUtc );
            byte[] data = Encoding.UTF8.GetBytes( $@"{{
  ""contentType"": ""{uploadObject.MimeType}"",
  ""bucketId"": ""{ApplicationData.BucketId}"",
  ""fileName"": ""{CleanUploadPath( uploadObject.UploadFilePath, true )}"",
  ""fileInfo"": {{
    ""sha512_filehash"": ""{uploadObject.CompleteSha512Hash}"",
    ""large_file_sha1"": ""{uploadObject.CompleteSha1Hash}"",
    ""src_last_modified_millis"": ""{dto.ToUnixTimeMilliseconds( )}""
  }},
  ""serverSideEncryption"": {{
    ""mode"": ""SSE-B2"",
    ""algorithm"": ""AES256""
  }}
}}" );

            JsonElement root = await GetBackBlazeGeneralClient( ).GetJsonResponse(
                uploadUri,
                HttpMethod.Post,
                await NewAuthToken( ),
                data,
                null
            );
            _log?.Debug( $"NewStartLargeFileURL Response: {root}" );

            uploadObject.FileId =
                root.GetProperty( "fileId" ).GetString( ) ??
                throw new InvalidB2Response(
                    uploadUri,
                    new NullReferenceException( "FileId" )
                );

            activity?.Stop( );
            return uploadObject;
        }

        internal async Task<UploadB2File> NewUploadLargeFilePartUrl( UploadB2File uploadObject ) {
            using Activity? activity = _source.StartActivity( "NewUploadLargeFilePartUrl" )?.Start( );

            string? uploadUri = _authorizationData.ApiUrl + "/b2api/v2/b2_get_upload_part_url";
            byte[] data = Encoding.UTF8.GetBytes( $"{{\"fileId\": \"{uploadObject.FileId}\"}}" );

            JsonElement root = await GetBackBlazeGeneralClient( ).GetJsonResponse(
                uploadUri,
                HttpMethod.Post,
                await NewAuthToken( ),
                data,
                null
            );
            _log?.Debug( $"NewUploadLargeFilePartUrl Response: {root}" );

            uploadObject.FileId =
                root.GetProperty( "fileId" ).GetString( ) ??
                throw new InvalidB2Response( uploadUri, new NullReferenceException( "FileId" ) );

            uploadObject.AuthorizationToken =
                root.GetProperty( "authorizationToken" ).GetString( ) ??
                throw new InvalidB2Response( uploadUri, new NullReferenceException( "AuthorizationToken" ) );

            uploadObject.UploadUrl =
                root.GetProperty( "uploadUrl" ).GetString( ) ??
                throw new InvalidB2Response( uploadUri, new NullReferenceException( "UploadUrl" ) );

            _log?.Debug( $"UploadUrl: {uploadObject.UploadUrl}" );

            activity?.Stop( );
            return uploadObject;
        }

        internal async Task UploadLargeFilePart( UploadB2FilePart upload, int thread ) {
            using Activity? activity = _source.StartActivity( "UploadLargeFilePart" )?.Start( );
            HttpRequestException? result;

            result = await GetBackBlazeGeneralClient( ).B2UploadPart( upload );
            if (result != null) { throw result; } else { ThreadStats[thread].Success++; }

            activity?.Stop( );
        }

        internal async Task FinishUploadLargeFile( UploadB2File uploadObject ) {
            using Activity? activity = _source.StartActivity( "FinishUploadLargeFile" )?.Start( );
            _log?.Debug( uploadObject );

            B2FinishLargeFileRequest finishLargeFileData = new( ) {
                fileId = uploadObject.FileId,
                partSha1Array = uploadObject.Sha1PartsList
                                    .OrderBy( x => x.Key )
                                    .Select( x => x.Value )
                                    .ToList( )
            };

            _log?.Debug( "finishLargeFileData: " + finishLargeFileData );

            await GetBackBlazeGeneralClient( ).SendStringContent(
                _authorizationData.ApiUrl + "/b2api/v2/b2_finish_large_file",
                HttpMethod.Post,
                await NewAuthToken( ),
                FinishLargeFileRequestToJson( finishLargeFileData )
            );

            foreach (UploadThreadStatistics stat in ThreadStats) {
                stat.ResetSleepStats( );
            }

            activity?.Stop( );
        }

        private string FinishLargeFileRequestToJson( B2FinishLargeFileRequest finishLargeFileData ) {
            using Activity? activity = _source.StartActivity( "FinishLargeFileRequestToJson" )?.Start( );

            DataContractJsonSerializer? serializer = new( typeof( B2FinishLargeFileRequest ) );

            using MemoryStream? stream = new( );
            serializer.WriteObject( stream, finishLargeFileData );

            activity?.Stop( );
            return Encoding.UTF8.GetString( stream.ToArray( ) );
        }

        #endregion LargeFileUploads
        // All Paths are relative to the bucket root.
        public string CleanUploadPath( string uploadFilePath, bool json ) {
            using Activity? activity = _source.StartActivity( "CleanUploadPath" )?.Start( );

            string uploadpath = uploadFilePath
                .Replace( "\\", "/" )
                .TrimStart( '/' );
            string cleanUri = "";

            if (json) {
                cleanUri = JsonSerializer
                            .Serialize( uploadpath, new JsonSerializerOptions( ) )
                            .Replace( "\\u0022", null )
                            .Replace( "\"", null )
                            .Replace( "\u0022", null );

            } else {
                List<char> dontEscape = new( );
                dontEscape.AddRange( "._-/~!$'()*;=:@ ".ToCharArray( ) );
                dontEscape.AddRange( UniquePassword.UpperCase );
                dontEscape.AddRange( UniquePassword.LowerCase );
                dontEscape.AddRange( UniquePassword.Numbers );

                foreach (char c in uploadpath.ToCharArray( )) {
                    cleanUri += dontEscape.Contains( c ) ? c : Uri.HexEscape( c );
                }
                cleanUri = cleanUri.Replace( ' ', '+' );
            }
            cleanUri = cleanUri.TrimEnd( '/' );

            activity?.Stop( );
            return cleanUri;
        }

        #endregion Uploads


        #region Downloads

        public async Task<B2DownloadResponse> DownloadFileID( string fileID, FileInfo outputPath ) {
            using Activity? activity = _source.StartActivity( "DownloadFileID" )?.Start( );

            // Get auth token - this also populates AuthorizationData and ensures we dont have a null url.
            string authToken = await NewAuthToken( );

            B2DownloadResponse response = await GetBackBlazeGeneralClient( ).B2DownloadFileById(
                fileID,
                outputPath,
                _authorizationData.DownloadUrl,
                authToken
            );

            activity?.Stop( );
            return response;
        }

        #endregion Downloads


        #region ListFiles

        public async Task<List<B2FileResponse>> ListFileVersions(
            string startFileName,
            string startFileId,
            int maxFileCount,
            bool singleCall,
            string prefix,
            List<B2FileResponse>? output = null
        ) {
            using Activity? activity = _source.StartActivity( "ListFileVersions" )?.Start( );

            string? listFileVersionsUri = _authorizationData.ApiUrl + "/b2api/v2/b2_list_file_versions";


            if (maxFileCount is < 0 or > 1000) {
                maxFileCount = 1000; // Maximum returned per transaction.
            }

            string fileVers = $"{{\"bucketId\":\"{ApplicationData.BucketId}\"" +
                              $",\"maxFileCount\": {maxFileCount}";
            if (string.IsNullOrWhiteSpace( startFileName ) == false) {
                fileVers += $",\"startFileName\":\"{startFileName}\"";
            }
            if (string.IsNullOrWhiteSpace( startFileId ) == false) {
                fileVers += string.IsNullOrWhiteSpace( startFileName ) ?
                throw new Exception( "Need startFileName to use startFileId" ) :
                $",\"startFileId\":\"{startFileId}\"";
            }
            if (string.IsNullOrWhiteSpace( prefix ) == false) {
                fileVers += $",\"prefix\":\"{prefix}\"";
            }
            fileVers += "}";

            byte[] data = Encoding.UTF8.GetBytes( fileVers );
            output ??= new List<B2FileResponse>( );

            JsonElement root = await GetBackBlazeGeneralClient( ).GetJsonResponse(
                listFileVersionsUri,
                HttpMethod.Post,
                await NewAuthToken( ),
                data,
                null
            );

            // Successfully deserialize B2FilesResponse or throw error.
            B2FilesResponse filesResponse =
                JsonSerializer.Deserialize<B2FilesResponse>( root.ToString( ) ) ??
                throw new InvalidB2Response( listFileVersionsUri, new NullReferenceException( "B2FilesResponse" ) );

            output.AddRange( filesResponse.files );
            if (string.IsNullOrWhiteSpace( filesResponse.nextFileId ) == false && singleCall == false) {
                output = await ListFileVersions( filesResponse.nextFileName, filesResponse.nextFileId, maxFileCount, singleCall, prefix, output );
            }

            activity?.Stop( );
            return output;
        }

        #endregion ListFiles


        #region DeleteFiles

        public async void DeleteFileVersion( string fileID, string fileName ) {
            using Activity? activity = _source.StartActivity( "DeleteFileVersion" )?.Start( );

            await GetBackBlazeGeneralClient( ).SendStringContent(
                _authorizationData.ApiUrl + "/b2api/v2/b2_delete_file_version",
                HttpMethod.Post,
                await NewAuthToken( ),
                $"{{\"fileName\":\"{fileName}\",\n\"fileId\":\"{fileID}\"}}"
            );

            activity?.Stop( );
        }

        #endregion DeleteFiles


        #region ExceptionHandling

        // This exception handling is temporary and will be ditched for well configured Polly policies at some point.

        public void HandleBackBlazeException(
            HttpRequestException webExcp,
            int errCount,
            int thread,
            ConcurrentStack<FilePartInfo> filePartQueue
        ) {
            _failureDetails[thread].PastFailureTime = _failureDetails[thread].FailureTime;
            _failureDetails[thread].FailureTime = DateTime.UtcNow;
            _failureDetails[thread].StatusCode = (webExcp.StatusCode == null) ? null : (int)webExcp.StatusCode;

            WriteHttpRequestExceptionInfo( webExcp, errCount, thread );
            HandleStatusCode( webExcp, _failureDetails[thread].StatusCode );
            ThreadStatusManager.AddSleepingThread( thread );
            bool stillAsleep = HandleRetryWait( thread, filePartQueue );

            if (stillAsleep == false) {
                ThreadStatusManager.RemoveSleepingThread( );
            }
        }

        internal void WriteHttpRequestExceptionInfo(
            HttpRequestException webExcp,
            int errCount,
            int thread
        ) {
            string logMessage = (errCount < ApplicationData.MaxErrors) ?
                $"Thread#{thread} An error has occurred while uploading large file parts. " +
                $"This is error number {errCount} for this request." :
                $"Thread#{thread} Failed to upload large file part.";
            if (_failureDetails[thread].StatusCode != null) {
                logMessage += $"\nStatus Code: {_failureDetails[thread].StatusCode}";
            }

            string expMsg = webExcp.Message;
            string innerExp = webExcp.InnerException?.ToString( ) ?? "";

            switch (true) {
                case true when _regexPatterns[0].Match( expMsg ).Success:
                    logMessage += "\n" + innerExp.Split( "\n" )[0][38..];
                    break;
                case true when _regexPatterns[1].Match( expMsg ).Success:
                    logMessage += $"\n {expMsg} ";
                    logMessage += webExcp.InnerException?.Message ?? "";
                    break;
                default:
                    string[] myKeys = new string[webExcp.Data.Count];
                    webExcp.Data.Keys.CopyTo( myKeys, 0 );

                    logMessage += $"\nMessage       : {webExcp.Message}";
                    logMessage += $"\nStatusCode    : {webExcp.StatusCode}";
                    logMessage += $"\nData          : {webExcp.Data}";
                    if (myKeys.Length > 0) { logMessage += "\nData          : "; }
                    for (int i = 0; i < webExcp.Data.Count; i++) {
                        logMessage += $"\n{i,-5}. '{myKeys[i]}' '{webExcp.Data[myKeys[i]]}'";
                    }
                    logMessage += $"\nHelpLink      : {webExcp.HelpLink}";
                    logMessage += $"\nSource        : {webExcp.Source}";
                    logMessage += $"\nTargetSite    : {webExcp.TargetSite}";
                    logMessage += $"\nHResult       : {webExcp.HResult}";
                    logMessage += $"\nStackTrace    : {webExcp.StackTrace}";
                    logMessage += $"\nInnerException: {webExcp.InnerException}";
                    break;
            }

            if (errCount < ApplicationData.MaxErrors) {
                _log?.Info( logMessage );
            } else {
                _log?.Error( logMessage );
            }
        }

        internal void HandleStatusCode(
            HttpRequestException webExcp,
            int? statusCode
        ) {
            switch (statusCode) {
                case 403:
                    _log?.Fatal( webExcp.Message );
                    throw new Exception( "Received StatusCode 403.", webExcp );
                default:
                    break;
            }
        }

        private bool HandleRetryWait(
            int thread,
            ConcurrentStack<FilePartInfo> filePartQueue
        ) {
            if (_failureDetails[thread].PastFailureTime != null) {
                if (_failureDetails[thread].PastFailureTime <= DateTime.Now.AddMinutes( -7 )) {
                    _log?.Debug( $"Thread#{thread} Previous error was 7 or more minutes ago. Resetting wait counter." );
                    _failureDetails[thread].RetryWaitTimer = _retryStartSec[_random.Next( 0, _retryStartSec.Length )];
                } else if (_failureDetails[thread].PastFailureTime <= DateTime.Now.AddMinutes( -6 )) {
                    _log?.Debug( $"Thread#{thread} Previous error was 6 or more minutes ago. Resetting wait counter to 15 seconds." );
                    _failureDetails[thread].RetryWaitTimer = 15;
                } else if (_failureDetails[thread].PastFailureTime <= DateTime.Now.AddMinutes( -5 )) {
                    _log?.Debug( $"Thread#{thread} Previous error was 5 or more minutes ago. Resetting wait counter to 31 seconds." );
                    _failureDetails[thread].RetryWaitTimer = 31;
                } else if (_failureDetails[thread].PastFailureTime <= DateTime.Now.AddMinutes( -4 )) {
                    _log?.Debug( $"Thread#{thread} Previous error was 4 or more minutes ago. Resetting wait counter to 63 seconds." );
                    _failureDetails[thread].RetryWaitTimer = 63;
                }
            }

            int sleepTime = _failureDetails[thread].RetryWaitTimer;

            _failureDetails[thread].RetryWaitTimer = 1 + (2 * _failureDetails[thread].RetryWaitTimer);
            _log?.Debug( $"Thread#{thread} Retry timer set to {_failureDetails[thread].RetryWaitTimer} seconds." );
            _log?.Debug( $"Thread#{thread} Sleeping for {sleepTime} seconds." );
            _log?.Debug( $"ActiveThreadCount:   {ThreadStatusManager.ActiveThreadCount}" );
            _log?.Debug( $"SleepingThreadCount: {ThreadStatusManager.SleepingThreadCount}" );

            int sleepCount = 0;
            while (filePartQueue.IsEmpty == false && sleepCount < sleepTime) {
                Thread.Sleep( 1000 ); // Wait before retrying.
                sleepCount++;
            }

            // Add Failure Stats
            ThreadStats[thread].Failure++;
            ThreadStats[thread].AddSleepTimer( sleepCount );

            return (sleepCount >= sleepTime);
        }

        #endregion ExceptionHandling

    }
}
