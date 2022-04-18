using log4net;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Cloud_ShareSync.Core.Logging.Telemetry {
    internal static class TelemetryExporterExtensions {
        internal static TracerProviderBuilder AddLogExporter( this TracerProviderBuilder builder, ILog? log ) {
            return builder?.AddProcessor( new BatchActivityExportProcessor( new TelemetryExporter( log ) ) ) ??
                throw new ArgumentNullException( nameof( builder ) );
        }
    }
}
