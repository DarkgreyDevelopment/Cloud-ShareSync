using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core {

    public static class SystemMemoryChecker {
        public static long Total { get; private set; }
        public static long Consumed { get; private set; }
        public static long Available => Total - Consumed;

        private static ILogger? s_log;

        public static void Inititalize( ILogger? log = null ) {
            s_log = log;
            Update( );
        }

        public static void Update( ) {
            Total = Process.GetCurrentProcess( ).WorkingSet64;
            Consumed = GC.GetTotalMemory( true );
            s_log?.LogDebug(
                "Memory Stats - Total: {string}, Consumed: {string}, Available: {string}",
                Total, Consumed, Available
            );
        }
    }
}
