using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private async Task UploadLargeFilePart( UploadB2FilePart upload, int thread ) {
            using Activity? activity = _source.StartActivity( "UploadLargeFilePart" )?.Start( );

            B2ThreadManager.ThreadStats[thread].Attempt++;
            HttpRequestException? result;
            result = await GetBackBlazeGeneralClient( ).B2UploadPart( upload );
            if (result != null) {
                throw result;
            } else { B2ThreadManager.ThreadStats[thread].Success++; }

            activity?.Stop( );
        }

    }
}
