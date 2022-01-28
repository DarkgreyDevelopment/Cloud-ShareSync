using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.SharedServices {

    public class SystemMemoryChecker {
        public static long Total { get; private set; }
        public static long Consumed { get; private set; }
        public static long Available => Total - Consumed;

        private static ILogger? s_log;

        public SystemMemoryChecker( ILogger? log = null ) {
            s_log = log;
        }

        public static void Update( ) {
            Total = Process.GetCurrentProcess( ).WorkingSet64;
            Consumed = GC.GetTotalMemory( true );

            string msg = $"Memory Stats - Total: {Total}, Consumed: {Consumed}, Available: {Available}";

            s_log?.LogDebug( "{string}", msg );
        }
    }
}
