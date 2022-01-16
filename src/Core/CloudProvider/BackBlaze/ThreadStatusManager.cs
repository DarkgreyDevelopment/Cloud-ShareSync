using log4net;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {
#nullable disable
    internal static class ThreadStatusManager {
        public static int ActiveThreadCount = 0;
        public static int SleepingThreadCount = 0;

        private static ILog s_log;

        public static void Init( ILog log ) {
            s_log = log;
        }

        public static void Reset( ) {
            s_log?.Debug( $"ActiveThreadCount:   {ActiveThreadCount}" );
            s_log?.Debug( $"SleepingThreadCount: {SleepingThreadCount}" );
            s_log?.Debug( "Resetting Thread Status." );
            ActiveThreadCount = 0;
            SleepingThreadCount = 0;
        }

        public static void AddActiveThread( int thread ) {
            Interlocked.Increment( ref ActiveThreadCount );
            s_log?.Debug( $"Thread#{thread} is active. There are {ActiveThreadCount} active threads." );
        }

        public static void RemoveActiveThread( ) {
            Interlocked.Decrement( ref ActiveThreadCount );
            s_log?.Debug( $"Decrementing active thread count by 1. There are {ActiveThreadCount} active threads." );
        }

        public static void AddSleepingThread( int thread ) {
            Interlocked.Increment( ref SleepingThreadCount );
            s_log?.Debug( $"Thread#{thread} is sleeping. There are {SleepingThreadCount} sleeping threads." );
        }

        public static void RemoveSleepingThread( ) {
            Interlocked.Decrement( ref SleepingThreadCount );
            s_log?.Debug( $"Decrementing sleeping thread count by 1. There are {SleepingThreadCount} sleeping threads." );
        }
    }
}
