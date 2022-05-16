using System.Text;
using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints {
    internal class ListFileVersions {
#pragma warning disable IDE1006 // Naming Styles - Names Match B2 fields
        public List<B2File> files { get; set; } = new( );
        public string? nextFileName { get; set; }
        public string? nextFileId { get; set; }
#pragma warning restore IDE1006 // Naming Styles - Names Match B2 fields

        public const string EndpointURI = "/b2api/v2/b2_list_file_versions";

        internal static async Task<ListFileVersions> CallApi(
            AuthorizeAccount authData,
            string bucketId,
            HttpClient client,
            int retryCount,
            string startFileName = "",
            string startFileId = "",
            int maxFileCount = 1000,
            string prefix = "",
            ILogger? log = null
        ) {
            string listUri = authData.apiUrl + EndpointURI;
            if (maxFileCount is < 0 or > 1000) {
                maxFileCount = 1000; // Maximum returned per transaction.
            }

            byte[] data = Encoding.UTF8.GetBytes(
                BuildRequestContent( bucketId, startFileName, startFileId, maxFileCount, prefix )
            );
            return DeserializeJsonDocument(
                await B2RequestHandler.ProcessB2Request(
                    HttpMethod.Post,
                    listUri,
                    authData.authorizationToken,
                    data,
                    null,
                    EndpointCalls.ListFileVersions,
                    retryCount,
                    client,
                    true,
                    log
                )
            );
        }

        private static string BuildRequestContent(
            string bucketId,
            string startFileName,
            string startFileId,
            int maxFileCount,
            string prefix
        ) {
            string fileVers = $"{{\"bucketId\":\"{bucketId}\"" +
                              $",\"maxFileCount\": {maxFileCount}";
            fileVers += AddStartFileName( startFileName );
            fileVers += AddStartFileId( startFileName, startFileId );
            fileVers += AddPrefix( prefix );

            return fileVers + "}";
        }

        private static string AddStartFileName( string startFileName ) =>
            string.IsNullOrWhiteSpace( startFileName ) == false ?
                $",\"startFileName\":\"{startFileName}\"" : string.Empty;

        private static string AddStartFileId(
            string startFileName,
            string startFileId
        ) => string.IsNullOrWhiteSpace( startFileId ) == false
                ? string.IsNullOrWhiteSpace( startFileName ) ?
                    throw new FailedB2RequestException( "Need startFileName to use startFileId" ) :
                    $",\"startFileId\":\"{startFileId}\"" :
                string.Empty;

        private static string AddPrefix( string prefix ) =>
            string.IsNullOrWhiteSpace( prefix ) == false ? $",\"prefix\":\"{prefix}\"" : string.Empty;


        private static ListFileVersions DeserializeJsonDocument( JsonDocument document ) =>
            (ListFileVersions)document.Deserialize( typeof( ListFileVersions ) )!;
    }
}
