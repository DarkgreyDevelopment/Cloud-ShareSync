﻿using System.Collections.Concurrent;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private void HandleRetryWait(
            int thread,
            ConcurrentStack<FilePartInfo> filePartQueue,
            B2ConcurrentStats concurrencyStats
        ) {

            if (B2ThreadManager.FailureDetails[thread].PastFailureTime != null) {
                if (B2ThreadManager.FailureDetails[thread].PastFailureTime <= DateTime.Now.AddMinutes( -5 )) {
                    _log?.LogDebug(
                        "Thread#{string} Previous error was 5 or more minutes ago. " +
                        "Resetting Failure Details.",
                        thread
                    );
                    B2ThreadManager.FailureDetails[thread].Reset( );
                } else if (B2ThreadManager.FailureDetails[thread].PastFailureTime <= DateTime.Now.AddMinutes( -4 )) {
                    _log?.LogDebug(
                        "Thread#{string} Previous error was 4 or more minutes ago. " +
                        "Setting wait counter to 15 seconds.",
                        thread
                    );
                    B2ThreadManager.FailureDetails[thread].RetryWaitTimer = 15;
                } else if (B2ThreadManager.FailureDetails[thread].PastFailureTime <= DateTime.Now.AddMinutes( -3 )) {
                    _log?.LogDebug(
                        "Thread#{string} Previous error was 3 or more minutes ago. " +
                        "Setting wait counter to 31 seconds.",
                        thread
                    );
                    B2ThreadManager.FailureDetails[thread].RetryWaitTimer = 31;
                }
            }

            int sleepTime = B2ThreadManager.FailureDetails[thread].RetryWaitTimer;

            B2ThreadManager.FailureDetails[thread].RetryWaitTimer = 1 +
                (2 * B2ThreadManager.FailureDetails[thread].RetryWaitTimer);
            _log?.LogDebug(
                "Thread#{string} Sleeping for {string} seconds.",
                thread,
                sleepTime
            );

            concurrencyStats.ThreadSleeping( thread );
            int sleepCount = 0;
            while (filePartQueue.IsEmpty == false && sleepCount < sleepTime) {
                Thread.Sleep( 1000 ); // Wait before retrying.
                sleepCount++;
            }

            // Add Failure Stats
            B2ThreadManager.ThreadStats[thread].Failure++;
            B2ThreadManager.ThreadStats[thread].AddSleepTimer( sleepCount );
            if (sleepTime <= sleepCount) { concurrencyStats.RemoveSleepingThread( thread ); }
        }

    }
}