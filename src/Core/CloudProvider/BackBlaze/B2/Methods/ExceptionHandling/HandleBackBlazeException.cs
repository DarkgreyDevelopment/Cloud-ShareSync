using System.Collections.Concurrent;
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
            ThreadManager.FailureDetails[thread].PastFailureTime = ThreadManager.FailureDetails[thread].FailureTime;
            ThreadManager.FailureDetails[thread].FailureTime = DateTime.UtcNow;
            ThreadManager.FailureDetails[thread].StatusCode = (webExcp.StatusCode == null) ?
                null : (int)webExcp.StatusCode;

            WriteHttpRequestExceptionInfo( webExcp, errCount, thread );
            HandleStatusCode( webExcp, ThreadManager.FailureDetails[thread].StatusCode );
            HandleRetryWait( thread, filePartQueue, concurrencyStats );
        }

    }
}
