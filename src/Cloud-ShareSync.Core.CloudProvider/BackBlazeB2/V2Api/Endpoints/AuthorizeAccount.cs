using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints {
    internal class AuthorizeAccount {
#nullable disable
#pragma warning disable IDE1006 // Naming Styles - Names Match B2 fields
        public int absoluteMinimumPartSize { get; set; }
        public string accountId { get; set; }
        public AuthorizeAccountAllowed allowed { get; set; }
        public string apiUrl { get; set; }
        public string authorizationToken { get; set; }
        public string downloadUrl { get; set; }
        public int recommendedPartSize { get; set; }
        public string s3ApiUrl { get; set; }
#pragma warning restore IDE1006 // Naming Styles - Names Match B2 fields
#nullable enable

        public const string AuthorizationURI = "https://api.backblazeb2.com/b2api/v2/b2_authorize_account";

        internal static async Task<AuthorizeAccount> CallApi(
            string credentials,
            HttpClient client,
            int retryCount,
            ILogger? log = null
        ) {
            return DeserializeJsonDocument(
                await B2RequestHandler.ProcessB2Request(
                    HttpMethod.Get,
                    AuthorizationURI,
                    "Basic " + credentials,
                    null,
                    null,
                    EndpointCalls.AuthorizeAccount,
                    retryCount,
                    client,
                    true,
                    log
                )
            );
        }

        private static AuthorizeAccount DeserializeJsonDocument(
            JsonDocument document
        ) => (AuthorizeAccount)document.Deserialize( typeof( AuthorizeAccount ) )!;
    }
}
