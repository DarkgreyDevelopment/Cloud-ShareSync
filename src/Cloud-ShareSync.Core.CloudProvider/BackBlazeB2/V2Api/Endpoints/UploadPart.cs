using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Cloud_ShareSync.Core.CloudProvider.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints {
    internal class UploadPart {
#pragma warning disable IDE1006 // Naming Styles - Names Match B2 fields
        public string fileId { get; set; } = string.Empty;
        public int partNumber { get; set; }
        public int contentLength { get; set; }
        public string contentSha1 { get; set; } = string.Empty;
        public string? contentMd5 { get; set; }
        //public ServerSideEncryptionState serverSideEncryption { get; set; } = new( );
        public long uploadTimestamp { get; set; }
#pragma warning restore IDE1006 // Naming Styles - Names Match B2 fields
        public DateTime UploadDateTime => DateTimeOffset.FromUnixTimeMilliseconds( uploadTimestamp ).DateTime;

        internal static async Task<UploadPart> CallApi(
            GetUploadPartUrl uploadPartUrlData,
            UploadFileInfo upload,
            FilePartInfo partInfo,
            HttpClient client,
            int attemptNumber,
            TimeSpan[] timeSpans,
            ILogger? log = null
        ) {
            List<KeyValuePair<string, string>> headers = CreateHeaders( upload, partInfo );
            return DeserializeJsonDocument(
                await B2RequestHandler.ProcessB2Request(
                    uploadPartUrlData.uploadUrl,
                    uploadPartUrlData.authorizationToken,
                    partInfo.Data,
                    headers,
                    client,
                    EndpointCalls.UploadPart,
                    attemptNumber,
                    timeSpans,
                    log
                )
            );
        }

        private static List<KeyValuePair<string, string>> CreateHeaders(
            UploadFileInfo upload,
            FilePartInfo partInfo
        ) {
            List<KeyValuePair<string, string>> headers = new( );
            headers.Add( new( "ContentType", upload.MimeType ) );
            headers.Add( new( "X-Bz-Part-Number", partInfo.PartNumber.ToString( ) ) );
            headers.Add( new( "X-Bz-Content-Sha1", partInfo.SHA1 ) );
            headers.Add( new( "X-Bz-Info-large_file_sha1", upload.SHA1 ) );
            return headers;
        }

        private static UploadPart DeserializeJsonDocument( JsonDocument document ) =>
            (UploadPart)document.Deserialize( typeof( UploadPart ) )!;

    }
}
