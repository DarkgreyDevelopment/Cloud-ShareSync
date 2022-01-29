using System.Diagnostics;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        internal async void DeleteFileVersion( string fileID, string fileName ) {
            using Activity? activity = _source.StartActivity( "DeleteFileVersion" )?.Start( );

            await GetBackBlazeGeneralClient( ).SendStringContent(
                _authorizationData.ApiUrl + "/b2api/v2/b2_delete_file_version",
                HttpMethod.Post,
                await NewAuthToken( ),
                $"{{\"fileName\":\"{fileName}\",\n\"fileId\":\"{fileID}\"}}"
            );

            activity?.Stop( );
        }

    }
}
