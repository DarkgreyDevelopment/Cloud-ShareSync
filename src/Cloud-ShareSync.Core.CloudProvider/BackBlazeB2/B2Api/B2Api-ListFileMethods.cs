using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2 {
    public partial class B2Api {
        public async Task<List<B2File>> ListBucketFiles( ) {
            List<B2File> output = new( );
            string? nextFileId;
            do {
                nextFileId = await GetFileVersions(
                    _initData.BucketId, GetHttpClient( ), _initData.MaxErrors, _log, output
                );
            } while (string.IsNullOrWhiteSpace( nextFileId ) == false);
            return output;
        }

        private async Task<string?> GetFileVersions(
            string bucketId,
            HttpClient client,
            int retryCount,
            ILogger? log,
            List<B2File> output
        ) {
            ListFileVersions fileVersion;
            try {
                fileVersion = await ListFileVersions.CallApi(
                    AuthToken, bucketId, client, retryCount, "", "", 1000, "", log
                );
            } catch (NewAuthTokenRequiredException) {
                await UpdateAuthData( );
                fileVersion = await ListFileVersions.CallApi(
                    AuthToken, bucketId, client, retryCount, "", "", 1000, "", log
                );
            }

            output.AddRange( fileVersion.files );
            return fileVersion.nextFileId;
        }
    }
}
