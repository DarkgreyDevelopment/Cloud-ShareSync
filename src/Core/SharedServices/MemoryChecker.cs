using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.SharedServices {

    internal static class SystemMemory {
        public static long Total { get; set; }
        public static long Consumed { get; set; }
        public static long Available => Total - Consumed;
    }

    public class MemoryChecker {
        public static readonly Process Proc;
        private static ILogger? s_log;

        static MemoryChecker( ) {
            Proc = Process.GetCurrentProcess( );
        }

        public MemoryChecker( ILogger? log = null ) {
            s_log = log;
        }

        public static void Update( ) {
            SystemMemory.Total = Proc.WorkingSet64;
            SystemMemory.Consumed = GC.GetTotalMemory( true );

            string msg = "Memory Usage:\n" +
                $"Total:     {SystemMemory.Total}\n" +
                $"Consumed:  {SystemMemory.Consumed}\n" +
                $"Available: {SystemMemory.Available}";

            s_log?.LogInformation( "{string}", msg );
        }
    }
}
