using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        internal async Task<List<B2FileResponse>> ListFileVersions(
            string startFileName,
            string startFileId,
            int maxFileCount,
            bool singleCall,
            string prefix,
            List<B2FileResponse>? output = null
        ) {
            using Activity? activity = _source.StartActivity( "ListFileVersions" )?.Start( );

            string? listFileVersionsUri = _authorizationData.ApiUrl + "/b2api/v2/b2_list_file_versions";


            if (maxFileCount is < 0 or > 1000) {
                maxFileCount = 1000; // Maximum returned per transaction.
            }

            string fileVers = $"{{\"bucketId\":\"{_applicationData.BucketId}\"" +
                              $",\"maxFileCount\": {maxFileCount}";
            if (string.IsNullOrWhiteSpace( startFileName ) == false) {
                fileVers += $",\"startFileName\":\"{startFileName}\"";
            }
            if (string.IsNullOrWhiteSpace( startFileId ) == false) {
                fileVers += string.IsNullOrWhiteSpace( startFileName ) ?
                throw new Exception( "Need startFileName to use startFileId" ) :
                $",\"startFileId\":\"{startFileId}\"";
            }
            if (string.IsNullOrWhiteSpace( prefix ) == false) {
                fileVers += $",\"prefix\":\"{prefix}\"";
            }
            fileVers += "}";

            byte[] data = Encoding.UTF8.GetBytes( fileVers );
            output ??= new List<B2FileResponse>( );

            JsonElement root = await GetBackBlazeGeneralClient( ).GetJsonResponse(
                listFileVersionsUri,
                HttpMethod.Post,
                await NewAuthToken( ),
                data,
                null
            );

            // Successfully deserialize B2FilesResponse or throw error.
            B2FilesResponse filesResponse =
                JsonSerializer.Deserialize<B2FilesResponse>( root.ToString( ) ) ??
                throw new InvalidB2Response( listFileVersionsUri, new NullReferenceException( "B2FilesResponse" ) );

            output.AddRange( filesResponse.files );
            if (string.IsNullOrWhiteSpace( filesResponse.nextFileId ) == false && singleCall == false) {
                output = await ListFileVersions( filesResponse.nextFileName, filesResponse.nextFileId, maxFileCount, singleCall, prefix, output );
            }

            activity?.Stop( );
            return output;
        }

    }
}
