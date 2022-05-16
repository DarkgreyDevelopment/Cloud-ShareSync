using System.Text;
using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints {
    internal class GetUploadPartUrl {
#nullable disable
#pragma warning disable IDE1006 // Naming Styles - Names Match B2 fields
        public string fileId { get; set; }
        public string uploadUrl { get; set; }
        public string authorizationToken { get; set; }
#pragma warning restore IDE1006 // Naming Styles - Names Match B2 fields
#nullable enable

        public const string EndpointURI = "/b2api/v2/b2_get_upload_part_url";

        internal static async Task<GetUploadPartUrl> CallApi(
            AuthorizeAccount authData,
            string fileId,
            HttpClient client,
            int retryCount,
            ILogger? log = null
        ) {
            string? uploadUri = authData.apiUrl + EndpointURI;
            byte[] data = Encoding.UTF8.GetBytes( $"{{\"fileId\": \"{fileId}\"}}" );
            return DeserializeJsonDocument(
                await B2RequestHandler.ProcessB2Request(
                    HttpMethod.Post,
                    uploadUri,
                    authData.authorizationToken,
                    data,
                    null,
                    EndpointCalls.GetUploadPartUrl,
                    retryCount,
                    client,
                    true,
                    log
                )
            );
        }

        private static GetUploadPartUrl DeserializeJsonDocument(
            JsonDocument document
        ) => (GetUploadPartUrl)document.Deserialize( typeof( GetUploadPartUrl ) )!;

    }
}
