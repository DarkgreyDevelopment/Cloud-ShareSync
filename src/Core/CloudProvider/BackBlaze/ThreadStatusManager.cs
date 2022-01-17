using log4net;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {
#nullable disable
    internal static class ThreadStatusManager {
        public static int ActiveThreadsCount = 0;
        public static int CompletedThreadsCount = 0;
        public static int SleepingThreadsCount = 0;

        private static ILog s_log;

        public static void Init( ILog log ) {
            s_log = log;
        }

        public static void Reset( ) {
            s_log?.Debug( $"ActiveThreadCount:    {ActiveThreadsCount}" );
            s_log?.Debug( $"CompletedThreadCount: {CompletedThreadsCount}" );
            s_log?.Debug( $"SleepingThreadCount:  {SleepingThreadsCount}" );
            s_log?.Debug( "Resetting Thread Status." );
            ActiveThreadsCount = 0;
            CompletedThreadsCount = 0;
            SleepingThreadsCount = 0;
        }

        public static void AddActiveThread( int thread ) {
            Interlocked.Increment( ref ActiveThreadsCount );
            s_log?.Debug( $"Thread#{thread} is active. There are {ActiveThreadsCount} active threads." );
        }

        public static void RemoveActiveThread( ) {
            Interlocked.Decrement( ref ActiveThreadsCount );
            s_log?.Debug( $"Decremented active thread count by 1. There are now {ActiveThreadsCount} active threads." );
        }

        public static void AddCompletedThread( int thread ) {
            Interlocked.Increment( ref CompletedThreadsCount );
            s_log?.Debug(
                $"Thread#{thread} has finished. There are {ActiveThreadsCount} active threads, " +
                $"{CompletedThreadsCount} completed threads, and {SleepingThreadsCount} sleeping threads."
            );
        }

        public static void AddSleepingThread( int thread ) {
            Interlocked.Increment( ref SleepingThreadsCount );
            s_log?.Debug( $"Thread#{thread} is sleeping. There are {SleepingThreadsCount} sleeping threads." );
        }

        public static void RemoveSleepingThread( ) {
            Interlocked.Decrement( ref SleepingThreadsCount );
            s_log?.Debug( $"Decremented sleeping thread count by 1. There are now {SleepingThreadsCount} sleeping threads." );
        }
    }
}
