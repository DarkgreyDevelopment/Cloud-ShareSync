using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        internal async Task<UploadB2File> NewSmallFileUploadUrl( UploadB2File uploadObject ) {
            using Activity? activity = _source.StartActivity( "NewSmallFileUploadUrl" )?.Start( );

            string? uploadUri = _authorizationData.ApiUrl + "/b2api/v2/b2_get_upload_url";
            DateTimeOffset dto = new( uploadObject.FilePath.LastWriteTimeUtc );
            byte[] data = Encoding.UTF8.GetBytes( $"{{\"bucketId\":\"{_applicationData.BucketId}\"}}" );

            JsonElement root = await GetBackBlazeGeneralClient( ).GetJsonResponse(
                uploadUri,
                HttpMethod.Post,
                await NewAuthToken( ),
                data,
                null
            );

            _log?.LogDebug( "NewSmallFileUploadUrl Response: {string}", root );

            uploadObject.UploadUrl =
                root.GetProperty( "uploadUrl" ).GetString( ) ??
                throw new InvalidB2Response(
                    uploadUri,
                    new NullReferenceException( "UploadUrl" )
                );

            uploadObject.AuthorizationToken =
                root.GetProperty( "authorizationToken" ).GetString( ) ??
                throw new InvalidB2Response(
                    uploadUri,
                    new NullReferenceException( "AuthorizationToken" )
                );

            activity?.Stop( );
            return uploadObject;
        }
    }
}
