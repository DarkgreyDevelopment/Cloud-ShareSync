using System.Text;
using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Cloud_ShareSync.Core.CloudProvider.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints {
    internal class StartLargeFile : B2File {

        public const string EndpointURI = "/b2api/v2/b2_start_large_file";

        internal static async Task<StartLargeFile> CallApi(
            AuthorizeAccount authData,
            string bucketId,
            UploadFileInfo upload,
            HttpClient client,
            int retryCount,
            ILogger? log = null
        ) {
            string? uploadUri = authData.apiUrl + EndpointURI;
            byte[] data = Encoding.UTF8.GetBytes(
$@"{{
  ""bucketId"": ""{bucketId}"",
  ""fileName"": ""{B2PathEscaper.CleanUploadPath( upload.UploadFilePath, true )}"",
  ""contentType"": ""{upload.MimeType}"",
  ""fileInfo"": {{
    ""sha512_filehash"": ""{upload.SHA512}"",
    ""large_file_sha1"": ""{upload.SHA1}"",
    ""src_last_modified_millis"": ""{upload.LastWriteTimeMS}""
  }}
}}"
            );
            return DeserializeJsonDocument(
                await B2RequestHandler.ProcessB2Request(
                    HttpMethod.Post,
                    uploadUri,
                    authData.authorizationToken,
                    data,
                    null,
                    EndpointCalls.StartLargeFile,
                    retryCount,
                    client,
                    true,
                    log
                )
            );
        }

        private static StartLargeFile DeserializeJsonDocument( JsonDocument document ) =>
            (StartLargeFile)document.Deserialize( typeof( StartLargeFile ) )!;
    }
}
