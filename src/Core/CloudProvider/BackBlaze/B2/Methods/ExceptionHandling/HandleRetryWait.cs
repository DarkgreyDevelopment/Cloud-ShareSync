using System.Collections.Concurrent;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private void HandleRetryWait(
            int thread,
            ConcurrentStack<FilePartInfo> filePartQueue,
            B2ConcurrentStats concurrencyStats
        ) {
            if (ThreadManager.FailureDetails[thread].PastFailureTime != null) {
                if (ThreadManager.FailureDetails[thread].PastFailureTime <= DateTime.Now.AddMinutes( -5 )) {
                    _log?.Debug(
                        $"Thread#{thread} Previous error was 5 or more minutes ago. " +
                        "Resetting Failure Details."
                    );
                    ThreadManager.FailureDetails[thread].Reset( );
                } else if (ThreadManager.FailureDetails[thread].PastFailureTime <= DateTime.Now.AddMinutes( -4 )) {
                    _log?.Debug(
                        $"Thread#{thread} Previous error was 4 or more minutes ago. " +
                        "Setting wait counter to 15 seconds."
                    );
                    ThreadManager.FailureDetails[thread].RetryWaitTimer = 15;
                } else if (ThreadManager.FailureDetails[thread].PastFailureTime <= DateTime.Now.AddMinutes( -3 )) {
                    _log?.Debug(
                        $"Thread#{thread} Previous error was 3 or more minutes ago. " +
                        "Setting wait counter to 31 seconds."
                    );
                    ThreadManager.FailureDetails[thread].RetryWaitTimer = 31;
                }
            }

            int sleepTime = ThreadManager.FailureDetails[thread].RetryWaitTimer;

            ThreadManager.FailureDetails[thread].RetryWaitTimer = 1 +
                (2 * ThreadManager.FailureDetails[thread].RetryWaitTimer);
            _log?.Debug( $"Thread#{thread} Sleeping for {sleepTime} seconds." );

            concurrencyStats.ThreadSleeping( thread );
            int sleepCount = 0;
            while (filePartQueue.IsEmpty == false && sleepCount < sleepTime) {
                Thread.Sleep( 1000 ); // Wait before retrying.
                sleepCount++;
            }

            // Add Failure Stats
            ThreadManager.ThreadStats[thread].Failure++;
            ThreadManager.ThreadStats[thread].AddSleepTimer( sleepCount );
            if (sleepTime <= sleepCount) { concurrencyStats.RemoveSleeping( thread ); }
        }

    }
}
