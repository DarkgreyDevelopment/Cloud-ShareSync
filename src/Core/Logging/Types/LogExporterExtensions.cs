using log4net;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Cloud_ShareSync.Core.Logging.Types {
    internal static class LogExporterExtensions {
        internal static TracerProviderBuilder AddLogExporter( this TracerProviderBuilder builder ) {
            return builder?.AddProcessor( new BatchActivityExportProcessor( new LogExporter( ) ) ) ??
                throw new ArgumentNullException( nameof( builder ) );
        }

        internal static TracerProviderBuilder AddLogExporter( this TracerProviderBuilder builder, ILog? log ) {
            return builder?.AddProcessor( new BatchActivityExportProcessor( new LogExporter( log ) ) ) ??
                throw new ArgumentNullException( nameof( builder ) );
        }
    }
}
