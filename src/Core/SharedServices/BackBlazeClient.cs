using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.SharedServices {

    public class BackBlazeHttpClient {

        HttpClient HttpClient { get; }

        public BackBlazeHttpClient( HttpClient client ) { HttpClient = client; }

        #region Generic Methods

        public async Task<JsonElement> GetJsonResponse( HttpRequestMessage request ) {

            using HttpResponseMessage? result = await HttpClient.SendAsync( request );
            result.EnsureSuccessStatusCode( );

            using Stream? contentstream = await result.Content.ReadAsStreamAsync( );

            return contentstream != null && contentstream.CanRead ?
                JsonDocument.Parse( contentstream ).RootElement :
                throw new InvalidB2Response(
                    request.RequestUri?.ToString( ) ?? "",
                    new NullReferenceException( "B2_Response" )
                );
        }

        public async Task<JsonElement> GetJsonResponse(
            string uri,
            HttpMethod method,
            string credentials,
            byte[] content,
            List<KeyValuePair<string, string>>? contentHeaders
        ) {
            try {
                HttpRequestMessage request = ClientUtilities.NewBackBlazeWebRequest(
                    uri,
                    method,
                    credentials,
                    content,
                    contentHeaders
                );

                using HttpResponseMessage? result = await HttpClient.SendAsync( request );
                result.EnsureSuccessStatusCode( );

                using Stream? contentstream = await result.Content.ReadAsStreamAsync( );

                return contentstream != null && contentstream.CanRead
                    ? JsonDocument.Parse( contentstream ).RootElement :
                    throw new InvalidB2Response( uri, new NullReferenceException( "B2_Response" ) );
            } catch (Exception e) {
                Console.Error.WriteLine( e );
                throw;
            }
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

            using HttpResponseMessage? result = await HttpClient.SendAsync( request );
            result.EnsureSuccessStatusCode( );
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
                using HttpResponseMessage? result = await HttpClient.SendAsync( request );
                result.EnsureSuccessStatusCode( );
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

            using HttpResponseMessage? result = await HttpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead
            );
            result.EnsureSuccessStatusCode( );

            response = GetDownloadResponseValues( result, response );

            // Write content stream out to filestream.
            byte[] buffer = new byte[4096];
            int count;
            Stream contentStream = await result.Content.ReadAsStreamAsync( ) ?? throw new InvalidB2Response(
                downloadUri,
                new NullReferenceException( "DownloadFileId_Response" )
            );

            using BinaryReader br = new( contentStream );
            using BinaryWriter writeFile = new( saveFile );
            while ((count = br.Read( buffer, 0, buffer.Length )) != 0)
                writeFile.Write( buffer, 0, count );

            return response;
        }

        private static B2DownloadResponse GetDownloadResponseValues(
            HttpResponseMessage responseMessage,
            B2DownloadResponse downloadResponse
        ) {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? headers = responseMessage.Headers.Distinct( );

            if (headers.Any( )) {
                KeyValuePair<string, IEnumerable<string>>? filenameHeader =
                    headers.Where( e => e.Key == "x-bz-file-name" ).FirstOrDefault( );
                if (filenameHeader != null) {
                    downloadResponse.FileName = filenameHeader.Value.Value.First( );
                }

                KeyValuePair<string, IEnumerable<string>>? fileidHeader =
                    headers.Where( e => e.Key == "x-bz-file-id" ).FirstOrDefault( );
                if (fileidHeader != null) {
                    downloadResponse.FileID = fileidHeader.Value.Value.First( );
                }

                KeyValuePair<string, IEnumerable<string>>? lastModifiedHeader =
                    headers.Where( e => e.Key == "x-bz-info-src_last_modified_millis" ).FirstOrDefault( );
                if (lastModifiedHeader != null) {
                    downloadResponse.LastModified = DateTimeOffset.FromUnixTimeMilliseconds(
                                                long.Parse( lastModifiedHeader.Value.Value.First( ) )
                                            ).DateTime;
                }

                KeyValuePair<string, IEnumerable<string>>? sha1contentHeader =
                    headers.Where( e => e.Key == "x-bz-content-sha1" ).FirstOrDefault( );
                if (sha1contentHeader != null) {
                    downloadResponse.Sha1FileHash = sha1contentHeader.Value.Value.First( );
                }

                KeyValuePair<string, IEnumerable<string>>? sha512contentHeader =
                    headers.Where( e => e.Key == "x-bz-info-sha512_filehash" ).FirstOrDefault( );
                if (sha512contentHeader != null) {
                    downloadResponse.Sha512FileHash = sha512contentHeader.Value.Value.First( );
                }
            }

            return downloadResponse;
        }

        internal async Task<AuthReturn> NewAuthReturn( string credentials ) {

            string authorizationURI = "https://api.backblazeb2.com/b2api/v2/b2_authorize_account";

            HttpRequestMessage request = ClientUtilities.NewBackBlazeWebRequest(
                                            authorizationURI,
                                            HttpMethod.Get,
                                            "Basic " + credentials
                                        );

            using HttpResponseMessage? result = await HttpClient.SendAsync( request );
            result.EnsureSuccessStatusCode( );

            // Send Auth Request & Read Response.
            using Stream? contentstream = await result.Content.ReadAsStreamAsync( );

            if (contentstream == null || contentstream.CanRead == false) {
                throw new InvalidB2Response(
                    authorizationURI,
                    new NullReferenceException( "Authorization_Response" )
                );
            }

            using JsonDocument document = JsonDocument.Parse( contentstream );

            JsonElement root = document.RootElement;

            AuthProcessData authProcessData = new(
                root.GetProperty( "accountId" ).GetString( ),
                root.GetProperty( "apiUrl" ).GetString( ),
                root.GetProperty( "s3ApiUrl" ).GetString( ),
                root.GetProperty( "downloadUrl" ).GetString( ),
                root.GetProperty( "recommendedPartSize" ).GetInt32( ),
                root.GetProperty( "absoluteMinimumPartSize" ).GetInt32( )
            );
            authProcessData.ValidateNotNull( );

            string? authorizationToken = root.GetProperty( "authorizationToken" ).GetString( );
            return new( authProcessData, authorizationToken );
        }

    }
}
