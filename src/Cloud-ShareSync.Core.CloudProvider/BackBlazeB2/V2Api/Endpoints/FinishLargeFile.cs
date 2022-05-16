using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints {
    internal class FinishLargeFile : B2File {

        public const string EndpointURI = "/b2api/v2/b2_finish_large_file";

        internal static async Task<FinishLargeFile> CallApi(
            AuthorizeAccount authData,
            B2FinishLargeFileRequest finishLargeFileData,
            HttpClient client,
            int retryCount,
            ILogger? log = null
        ) {
            string uploadUri = authData.apiUrl + EndpointURI;
            return DeserializeJsonDocument(
                await B2RequestHandler.ProcessB2Request(
                    HttpMethod.Post,
                    uploadUri,
                    authData.authorizationToken,
                    finishLargeFileData.ToString( ),
                    null,
                    EndpointCalls.FinishLargeFile,
                    retryCount,
                    client,
                    true,
                    log
                )
            );
            throw new FailedB2RequestException( );
        }

        private static FinishLargeFile DeserializeJsonDocument( JsonDocument document ) =>
            (FinishLargeFile)document.Deserialize( typeof( FinishLargeFile ) )!;
    }
}
