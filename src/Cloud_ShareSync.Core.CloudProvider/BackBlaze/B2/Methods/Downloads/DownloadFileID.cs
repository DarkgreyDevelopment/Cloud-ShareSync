using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        internal async Task<B2DownloadResponse> DownloadFileID( string fileID, FileInfo outputPath ) {
            using Activity? activity = _source.StartActivity( "DownloadFileID" )?.Start( );

            // Get auth token - this also populates AuthorizationData and ensures we dont have a null url.
            string authToken = await NewAuthToken( );

            B2DownloadResponse response = await GetBackBlazeGeneralClient( ).B2DownloadFileById(
                fileID,
                outputPath,
                _authorizationData.DownloadUrl,
                authToken
            );

            activity?.Stop( );
            return response;
        }

    }
}
