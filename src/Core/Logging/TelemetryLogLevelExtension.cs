using log4net;
using log4net.Core;

namespace Cloud_ShareSync.Core.Logging {
#nullable disable
    public static class TelemetryLogLevelExtension {

        public static readonly Level TelemetryLevel = new( 125000, "Telemetry" );

        public static void Telemetry( this ILog log, string message, Exception exception ) {
            log.Logger.Log( typeof( TelemetryLogLevelExtension ), TelemetryLevel, message, exception );
        }

        public static void Telemetry( this ILog log, string message ) { log.Telemetry( message, null ); }
    }
#nullable enable
}
