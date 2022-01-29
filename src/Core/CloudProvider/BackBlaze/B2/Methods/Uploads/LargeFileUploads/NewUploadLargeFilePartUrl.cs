using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private async Task<UploadB2File> NewUploadLargeFilePartUrl( UploadB2File uploadObject ) {
            using Activity? activity = _source.StartActivity( "NewUploadLargeFilePartUrl" )?.Start( );

            string? uploadUri = _authorizationData.ApiUrl + "/b2api/v2/b2_get_upload_part_url";
            byte[] data = Encoding.UTF8.GetBytes( $"{{\"fileId\": \"{uploadObject.FileId}\"}}" );

            JsonElement root = await GetBackBlazeGeneralClient( ).GetJsonResponse(
                uploadUri,
                HttpMethod.Post,
                await NewAuthToken( ),
                data,
                null
            );
            _log?.Debug( $"NewUploadLargeFilePartUrl Response: {root}" );

            uploadObject.FileId =
                root.GetProperty( "fileId" ).GetString( ) ??
                throw new InvalidB2Response( uploadUri, new NullReferenceException( "FileId" ) );

            uploadObject.AuthorizationToken =
                root.GetProperty( "authorizationToken" ).GetString( ) ??
                throw new InvalidB2Response( uploadUri, new NullReferenceException( "AuthorizationToken" ) );

            uploadObject.UploadUrl =
                root.GetProperty( "uploadUrl" ).GetString( ) ??
                throw new InvalidB2Response( uploadUri, new NullReferenceException( "UploadUrl" ) );

            _log?.Debug( $"UploadUrl: {uploadObject.UploadUrl}" );

            activity?.Stop( );
            return uploadObject;
        }

    }
}
