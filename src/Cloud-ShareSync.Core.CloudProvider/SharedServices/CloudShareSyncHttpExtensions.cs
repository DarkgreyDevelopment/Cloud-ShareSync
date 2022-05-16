using System.Text;
using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.SharedServices {
    internal static class CloudShareSyncHttpExtensions {

        #region HttpResponseMessage

        internal static JsonDocument ReadJsonContentStream(
            this HttpResponseMessage result,
            string uri,
            string call,
            ILogger? log
        ) {
            using Stream contentStream = result.ReadContentStream( uri, call );
            JsonDocument document = JsonDocument.Parse( contentStream );
            LogJsonDocument( document, log );
            return document;
        }

        internal static Stream ReadContentStream(
            this HttpResponseMessage result,
            string uri,
            string call
        ) => result.Content.ReadAsStream( ) ??
             throw new InvalidB2Response( uri, new NullReferenceException( call ) );

        private static void LogJsonDocument( JsonDocument document, ILogger? log ) {
            if (log == null || log.IsEnabled( LogLevel.Debug ) == false) { return; }

            using MemoryStream ms = new( );
            using Utf8JsonWriter writer = new( ms, new( ) { Indented = true } );
            writer.WriteStartObject( );

            foreach (JsonProperty property in document.RootElement.EnumerateObject( )) {
                property.WriteTo( writer );
            }

            writer.WriteEndObject( );
            writer.Flush( );
            log.LogDebug( "{string}", Encoding.UTF8.GetString( ms.ToArray( ) ) );
        }

        #endregion HttpResponseMessage


        #region HttpRequestMessage

        internal static async Task<HttpResponseMessage> SendAsyncRequest(
            this HttpRequestMessage request,
            HttpClient client,
            bool readResponseContent = true
        ) {
            HttpResponseMessage result = await client.SendAsync(
                request,
                readResponseContent ?
                    HttpCompletionOption.ResponseContentRead :
                    HttpCompletionOption.ResponseHeadersRead
            );
            return result;
        }

        #endregion HttpRequestMessage

    }
}
