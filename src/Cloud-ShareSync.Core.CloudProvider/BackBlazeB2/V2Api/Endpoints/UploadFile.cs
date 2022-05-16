using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Cloud_ShareSync.Core.CloudProvider.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints {
    internal class UploadFile : B2File {
        // Will have "/b2api/v2/b2_upload_file" in uri.

        internal static async Task<UploadFile> CallApi(
            GetUploadUrl uploadUrlData,
            UploadFileInfo upload,
            HttpClient client,
            int attemptNumber,
            TimeSpan[] timeSpans,
            ILogger? log = null
        ) {
            string dto = new DateTimeOffset( upload.FilePath.LastWriteTimeUtc )
                 .ToUnixTimeMilliseconds( )
                 .ToString( );
            byte[] data = File.ReadAllBytes( upload.FilePath.FullName );
            List<KeyValuePair<string, string>> headers = CreateHeaders( upload, dto );
            return DeserializeJsonDocument(
                await B2RequestHandler.ProcessB2Request(
                    uploadUrlData.uploadUrl,
                    uploadUrlData.authorizationToken,
                    data,
                    headers,
                    client,
                    EndpointCalls.UploadFile,
                    attemptNumber,
                    timeSpans,
                    log
                )
            );
        }

        private static List<KeyValuePair<string, string>> CreateHeaders( UploadFileInfo upload, string dto ) {
            List<KeyValuePair<string, string>> headers = new( );
            headers.Add( new( "ContentType", upload.MimeType ) );
            headers.Add( new( "X-Bz-File-Name", B2PathEscaper.CleanUploadPath( upload.UploadFilePath, false ) ) );
            headers.Add( new( "X-Bz-Content-Sha1", upload.SHA1 ) );
            headers.Add( new( "X-Bz-Info-src_last_modified_millis", dto ) );
            headers.Add( new( "X-Bz-Info-sha512_filehash", upload.SHA512 ) );
            //headers.Add( "X-Bz-Info-Author", "Cloud-ShareSync" );
            //headers.Add( "X-Bz-Server-Side-Encryption", "AES256" );
            return headers;
        }

        private static UploadFile DeserializeJsonDocument( JsonDocument document ) =>
            (UploadFile)document.Deserialize( typeof( UploadFile ) )!;
    }
}
