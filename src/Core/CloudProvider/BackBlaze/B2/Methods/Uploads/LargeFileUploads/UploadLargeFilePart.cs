using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.SharedServices;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private async Task UploadLargeFilePart( UploadB2FilePart upload, int thread ) {
            using Activity? activity = _source.StartActivity( "UploadLargeFilePart" )?.Start( );
            HttpRequestException? result;
            ThreadManager.ThreadStats[thread].Attempt++;
            SystemMemoryChecker.Update( );
            result = await GetBackBlazeGeneralClient( ).B2UploadPart( upload );
            if (result != null) {
                SystemMemoryChecker.Update( );
                throw result;
            } else { ThreadManager.ThreadStats[thread].Success++; }

            activity?.Stop( );
        }

    }
}
