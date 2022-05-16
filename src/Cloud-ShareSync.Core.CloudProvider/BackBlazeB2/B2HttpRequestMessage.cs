using System.Net.Http.Headers;
using System.Text;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2 {
    internal class B2HttpRequestMessage : HttpRequestMessage {

        public const string BackBlazeUserAgent = "Cloud-ShareSync_BackBlazeB2/0.7.0-prerelease+dotnet/6.0";

        public B2HttpRequestMessage(
            HttpMethod method,
            string uri,
            string token,
            string? range
        ) : base( method, uri ) {
            _ = Headers.TryAddWithoutValidation( "Authorization", token );
            _ = Headers.TryAddWithoutValidation( "UserAgent", BackBlazeUserAgent );
            if (range != null) { Headers.Add( "Range", $"bytes={range}" ); }
        }

        public B2HttpRequestMessage(
            HttpMethod method,
            string uri,
            string token,
            string content,
            List<KeyValuePair<string, string>>? contentHeaders = null,
            string? range = null
        ) : this( method, uri, token, range ) {
            Content = new StringContent( content, Encoding.UTF8, "application/json" );
            Content.Headers.ContentLength = content.Length;
            if (contentHeaders != null) {
                AddContentHeaders( contentHeaders );
            }
        }

        public B2HttpRequestMessage(
            HttpMethod method,
            string uri,
            string token,
            byte[] content,
            List<KeyValuePair<string, string>>? contentHeaders = null,
            string? range = null
        ) : this( method, uri, token, range ) {
            Content = new ByteArrayContent( content );
            Content.Headers.ContentLength = content.Length;
            if (contentHeaders != null) {
                AddContentHeaders( contentHeaders );
            }
        }

        private static KeyValuePair<string, string> GetContentTypeHeader(
            List<KeyValuePair<string, string>> contentHeaders
        ) => contentHeaders
            .Where( x => x.Key == "ContentType" )?
            .FirstOrDefault( )
            ?? new( "", "" );

        private void AddContentHeaders(
            List<KeyValuePair<string, string>> contentHeaders
        ) {
            SetContentTypeHeader(
                GetContentTypeHeader( contentHeaders ),
                contentHeaders
            );
            foreach (KeyValuePair<string, string> header in contentHeaders) {
                Content?.Headers.Add( header.Key, header.Value );
            }
        }

        private void SetContentTypeHeader(
            KeyValuePair<string, string> contentType,
            List<KeyValuePair<string, string>> contentHeaders
        ) {
            if (
                string.IsNullOrWhiteSpace( contentType.Value ) == false &&
                Content?.Headers != null
            ) {
                Content.Headers.ContentType = new MediaTypeHeaderValue( contentType.Value );
                if (contentHeaders.Contains( contentType )) {
                    _ = contentHeaders.Remove( contentType );
                }
            }
        }
    }
}
