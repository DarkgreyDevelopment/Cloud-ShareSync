using System.Net.Http.Headers;
using System.Text;

namespace Cloud_ShareSync.SharedServices {
    internal class ClientUtilities {

        #region BackBlazeB2

        internal const string BackBlazeUserAgent = "Cloud-ShareSync_BackBlazeB2/0.0.1+dotnet/6.0";

        private static HttpRequestMessage NewBackBlazeHttpRequestMessage(
            string uri,
            HttpMethod method,
            string credentials
        ) {

            HttpRequestMessage requestMessage = new( method, uri );
            // Add Authorization Headers
            _ = requestMessage.Headers.TryAddWithoutValidation( "Authorization", credentials );
            // Add UserAgent Headers
            _ = requestMessage.Headers.TryAddWithoutValidation( "UserAgent", BackBlazeUserAgent );

            return requestMessage;
        }

        internal static HttpRequestMessage NewBackBlazeWebRequest(
            string uri,
            HttpMethod method,
            string credentials
        ) {
            return NewBackBlazeHttpRequestMessage( uri, method, credentials );
        }

        internal static HttpRequestMessage NewBackBlazeWebRequest(
            string uri,
            HttpMethod method,
            string credentials,
            byte[] content,
            List<KeyValuePair<string, string>>? contentHeaders = null
        ) {
            // Create Initial Request
            HttpRequestMessage requestMessage = NewBackBlazeHttpRequestMessage( uri, method, credentials );
            // Create request content
            requestMessage.Content = new ByteArrayContent( content );
            // Set request Content-Length
            requestMessage.Content.Headers.ContentLength = content.Length;
            if (contentHeaders != null) {
                requestMessage = AddContentHeaders( requestMessage, contentHeaders );
            }

            return requestMessage;
        }

        internal static HttpRequestMessage NewBackBlazeWebRequest(
            string uri,
            HttpMethod method,
            string credentials,
            string content,
            List<KeyValuePair<string, string>>? contentHeaders = null
        ) {

            // Create Initial Request
            HttpRequestMessage requestMessage = NewBackBlazeHttpRequestMessage( uri, method, credentials );
            // Create request content
            requestMessage.Content = new StringContent( content, Encoding.UTF8, "application/json" );
            // Set request Content-Length
            requestMessage.Content.Headers.ContentLength = content.Length;
            if (contentHeaders != null) {
                requestMessage = AddContentHeaders( requestMessage, contentHeaders );
            }

            return requestMessage;
        }

        #endregion BackBlazeB2

        internal static HttpRequestMessage AddContentHeaders(
            HttpRequestMessage request,
            List<KeyValuePair<string, string>> contentHeaders
        ) {

            KeyValuePair<string, string> contentType = GetContentTypeHeader( contentHeaders );
            SetContentTypeHeader(
                contentType,
                request,
                contentHeaders
            );

            foreach (KeyValuePair<string, string> header in contentHeaders) {
                request.Content?.Headers.Add( header.Key, header.Value );
            }

            return request;
        }

        private static KeyValuePair<string, string> GetContentTypeHeader(
            List<KeyValuePair<string, string>> contentHeaders
        ) {
            return contentHeaders
                    .Where( x => x.Key == "ContentType" )?
                    .FirstOrDefault( )
                    ?? new( "", "" );
        }

        private static void SetContentTypeHeader(
            KeyValuePair<string, string> contentType,
            HttpRequestMessage request,
            List<KeyValuePair<string, string>> contentHeaders
        ) {
            if (string.IsNullOrWhiteSpace( contentType.Value ) == false &&
                request.Content?.Headers.ContentType != null
            ) {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue( contentType.Value );
                if (contentHeaders.Contains( contentType )) {
                    _ = contentHeaders.Remove( contentType );
                }
            }
        }

    }
}
