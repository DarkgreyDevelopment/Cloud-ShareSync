using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.SharedServices {

    internal class BackBlazeHttpClient {

        private HttpClient HttpClient { get; }

        public BackBlazeHttpClient( HttpClient client ) { HttpClient = client; }

        #region Generic Methods

        public async Task<JsonElement> GetJsonResponse( HttpRequestMessage request ) {

            using HttpResponseMessage result = await SendAsyncRequest( request );
            Stream contentStream = await ReadContentStream(
                result,
                request.RequestUri?.ToString( ) ?? "",
                "B2_Response"
            );

            return JsonDocument.Parse( contentStream ).RootElement;
        }

        public async Task<JsonElement> GetJsonResponse(
            string uri,
            HttpMethod method,
            string credentials,
            byte[] content,
            List<KeyValuePair<string, string>>? contentHeaders
        ) {
            HttpRequestMessage request = ClientUtilities.NewBackBlazeWebRequest(
                uri,
                method,
                credentials,
                content,
                contentHeaders
            );

            using HttpResponseMessage result = await SendAsyncRequest( request );

            using Stream contentStream = await ReadContentStream( result, uri, "B2_Response" );

            return JsonDocument.Parse( contentStream ).RootElement;
        }

        public async Task SendStringContent(
            string uri,
            HttpMethod method,
            string credentials,
            string content
        ) {
            HttpRequestMessage request = ClientUtilities.NewBackBlazeWebRequest(
                uri,
                method,
                credentials,
                content,
                null
            );
            using HttpResponseMessage result = await SendAsyncRequest( request );
        }

        #endregion Generic Methods

        public async Task<HttpRequestException?> B2UploadPart( UploadB2FilePart uploadObject ) {

            List<KeyValuePair<string, string>> headers = new( );
            headers.Add( new( "ContentType", uploadObject.MimeType ) );
            headers.Add( new( "X-Bz-Part-Number", uploadObject.PartNumber ) );
            headers.Add( new( "X-Bz-Content-Sha1", uploadObject.PartSha1Hash ) );
            headers.Add( new( "X-Bz-Info-large_file_sha1", uploadObject.CompleteSha1Hash ) );

            HttpRequestMessage request = ClientUtilities.NewBackBlazeWebRequest(
                uploadObject.UploadUrl,
                HttpMethod.Post,
                uploadObject.AuthorizationToken,
                uploadObject.Content,
                headers
            );

            try {
                using HttpResponseMessage result = await SendAsyncRequest( request );
                return null;
            } catch (HttpRequestException ex) {
                return ex;
            }
        }

        public async Task<B2DownloadResponse> B2DownloadFileById(
            string fileID,
            FileInfo outputPath,
            string? downloadUrl,
            string authToken
        ) {
            B2DownloadResponse response = new( ) {
                OutputPath = outputPath,
                FileID = fileID
            };

            string downloadUri = downloadUrl + "/b2api/v2/b2_download_file_by_id";
            string body = $"{{\"fileId\":\"{fileID}\"}}";

            using FileStream saveFile = new(
                                            outputPath.FullName,
                                            FileMode.Create,
                                            FileAccess.Write,
                                            FileShare.Read,
                                            10240,
                                            FileOptions.Asynchronous
                                        );

            HttpRequestMessage request = ClientUtilities.NewBackBlazeWebRequest(
                                            downloadUri,
                                            HttpMethod.Post,
                                            authToken,
                                            body,
                                            null
                                        );

            using HttpResponseMessage result = await SendAsyncRequest( request, false );

            response = GetDownloadResponseValues( result, response );

            // Write content stream out to filestream.
            using Stream contentStream = await ReadContentStream( result, downloadUri, "DownloadFileId_Response" );
            WriteSmallFile( contentStream, saveFile );

            return response;
        }

        private static void WriteSmallFile(
            Stream contentStream,
            FileStream saveFile
        ) {
            int count;
            byte[] buffer = new byte[4096];
            using BinaryReader br = new( contentStream );
            using BinaryWriter writeFile = new( saveFile );
            while ((count = br.Read( buffer, 0, buffer.Length )) != 0) {
                writeFile.Write( buffer, 0, count );
            }
        }

        private static B2DownloadResponse GetDownloadResponseValues(
            HttpResponseMessage responseMessage,
            B2DownloadResponse downloadResponse
        ) {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = responseMessage.Headers.Distinct( );

            if (headers.Any( )) {
                GetFileNameHeader( headers, downloadResponse );
                GetFileIdHeader( headers, downloadResponse );
                GetLastModifiedHeader( headers, downloadResponse );
                GetSha1contentHeader( headers, downloadResponse );
                GetSha512ContentHeader( headers, downloadResponse );
            }

            return downloadResponse;
        }

        private static void GetFileNameHeader(
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
            B2DownloadResponse downloadResponse
        ) {
            KeyValuePair<string, IEnumerable<string>>? filenameHeader =
                headers.FirstOrDefault( e => e.Key == "x-bz-file-name" );
            if (filenameHeader != null) {
                downloadResponse.FileName = filenameHeader.Value.Value.First( );
            }
        }

        private static void GetFileIdHeader(
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
            B2DownloadResponse downloadResponse
        ) {
            KeyValuePair<string, IEnumerable<string>>? fileidHeader =
                headers.FirstOrDefault( e => e.Key == "x-bz-file-id" );
            if (fileidHeader != null) {
                downloadResponse.FileID = fileidHeader.Value.Value.First( );
            }
        }

        private static void GetLastModifiedHeader(
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
            B2DownloadResponse downloadResponse
        ) {
            KeyValuePair<string, IEnumerable<string>>? lastModifiedHeader =
                headers.FirstOrDefault( e => e.Key == "x-bz-info-src_last_modified_millis" );
            if (lastModifiedHeader != null) {
                downloadResponse.LastModified = DateTimeOffset.FromUnixTimeMilliseconds(
                                            long.Parse( lastModifiedHeader.Value.Value.First( ) )
                                        ).DateTime;
            }
        }

        private static void GetSha1contentHeader(
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
            B2DownloadResponse downloadResponse
        ) {
            KeyValuePair<string, IEnumerable<string>>? sha1contentHeader =
                headers.FirstOrDefault( e => e.Key == "x-bz-content-sha1" );
            if (sha1contentHeader != null) {
                downloadResponse.Sha1FileHash = sha1contentHeader.Value.Value.First( );
            }
        }

        private static void GetSha512ContentHeader(
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
            B2DownloadResponse downloadResponse
        ) {
            KeyValuePair<string, IEnumerable<string>>? sha512contentHeader =
                headers.FirstOrDefault( e => e.Key == "x-bz-info-sha512_filehash" );
            if (sha512contentHeader != null) {
                downloadResponse.Sha512FileHash = sha512contentHeader.Value.Value.First( );
            }
        }

        internal async Task<AuthReturn> NewAuthReturn( string credentials ) {

            string authorizationURI = "https://api.backblazeb2.com/b2api/v2/b2_authorize_account";

            HttpRequestMessage request = ClientUtilities.NewBackBlazeWebRequest(
                                            authorizationURI,
                                            HttpMethod.Get,
                                            "Basic " + credentials
                                        );

            using HttpResponseMessage result = await SendAsyncRequest( request );

            // Send Auth Request & Read Response.
            using Stream contentStream = await ReadContentStream( result, authorizationURI, "Authorization_Response" );

            using JsonDocument document = JsonDocument.Parse( contentStream );
            return GetAuthReturnFromJson( document );
        }

        internal static AuthReturn GetAuthReturnFromJson( JsonDocument document ) {
            JsonElement root = document.RootElement;
            AuthProcessData authProcessData = new(
                root.GetProperty( "accountId" ).GetString( ),
                root.GetProperty( "apiUrl" ).GetString( ),
                root.GetProperty( "s3ApiUrl" ).GetString( ),
                root.GetProperty( "downloadUrl" ).GetString( ),
                root.GetProperty( "recommendedPartSize" ).GetInt32( ),
                root.GetProperty( "absoluteMinimumPartSize" ).GetInt32( )
            );
            string? authorizationToken = root.GetProperty( "authorizationToken" ).GetString( );
            return new( authProcessData, authorizationToken );
        }

        internal static async Task<Stream> ReadContentStream(
            HttpResponseMessage result,
            string uri,
            string call
        ) => await result.Content.ReadAsStreamAsync( ) ??
             throw new InvalidB2Response( uri, new NullReferenceException( call ) );

        internal async Task<HttpResponseMessage> SendAsyncRequest( HttpRequestMessage request, bool readContent = true ) {
            using HttpResponseMessage result = await HttpClient.SendAsync(
                request,
                readContent ?
                    HttpCompletionOption.ResponseContentRead :
                    HttpCompletionOption.ResponseHeadersRead
            );
            _ = result.EnsureSuccessStatusCode( );
            return result;
        }

    }
}
