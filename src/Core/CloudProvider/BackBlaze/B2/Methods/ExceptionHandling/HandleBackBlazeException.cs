using System.Collections.Concurrent;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        // This exception handling is temporary and will be ditched for well configured Polly policies at some point.
        private void HandleBackBlazeException(
            HttpRequestException webExcp,
            int errCount,
            int thread,
            ConcurrentStack<FilePartInfo> filePartQueue,
            B2ConcurrentStats concurrencyStats
        ) {
            B2ThreadManager.FailureDetails[thread].PastFailureTime = B2ThreadManager.FailureDetails[thread].FailureTime;
            B2ThreadManager.FailureDetails[thread].FailureTime = DateTime.UtcNow;
            B2ThreadManager.FailureDetails[thread].StatusCode = (webExcp.StatusCode == null) ?
                null : (int)webExcp.StatusCode;

            WriteHttpRequestExceptionInfo( webExcp, errCount, thread );
            HandleStatusCode( webExcp, B2ThreadManager.FailureDetails[thread].StatusCode );
            HandleRetryWait( thread, filePartQueue, concurrencyStats );
        }

    }
}
