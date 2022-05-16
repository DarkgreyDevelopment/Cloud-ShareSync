using System.Text;
using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints {
    internal class GetUploadUrl {
#nullable disable
#pragma warning disable IDE1006 // Naming Styles - Names Match B2 fields
        public string bucketId { get; set; }
        public string uploadUrl { get; set; }
        public string authorizationToken { get; set; }
#pragma warning restore IDE1006 // Naming Styles - Names Match B2 fields
#nullable enable

        public const string EndpointURI = "/b2api/v2/b2_get_upload_url";

        internal static async Task<GetUploadUrl> CallApi(
            AuthorizeAccount authData,
            string bucketId,
            HttpClient client,
            int retryCount,
            ILogger? log = null
        ) {
            string? uploadUri = authData.apiUrl + EndpointURI;
            byte[] data = Encoding.UTF8.GetBytes( $"{{\"bucketId\":\"{bucketId}\"}}" );
            return DeserializeJsonDocument(
                await B2RequestHandler.ProcessB2Request(
                    HttpMethod.Post,
                    uploadUri,
                    authData.authorizationToken,
                    data,
                    null,
                    EndpointCalls.GetUploadUrl,
                    retryCount,
                    client,
                    true,
                    log
                )
            );
        }

        private static GetUploadUrl DeserializeJsonDocument( JsonDocument document ) =>
            (GetUploadUrl)document.Deserialize( typeof( GetUploadUrl ) )!;
    }
}
