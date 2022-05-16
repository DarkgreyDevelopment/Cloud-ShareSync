using System.Diagnostics;
using System.Text.Json;
using log4net;
using OpenTelemetry;

namespace Cloud_ShareSync.Core.Logging.Telemetry {
    internal class TelemetryExporter : BaseExporter<Activity> {

        public TelemetryExporter( string name = "LogExporter" ) : this( null, name ) { }

        public TelemetryExporter(
            ILog? log,
            string name = "LogExporter"
        ) {
            _name = name;
            _log = log;
        }

        private readonly string _name;
        private readonly ILog? _log;

        public override ExportResult Export( in Batch<Activity> batch ) {

            using IDisposable scope = SuppressInstrumentationScope.Begin( );
            foreach (Activity activity in batch) {
                string jsonString = JsonSerializer.Serialize(
                    activity,
                    new JsonSerializerOptions( )
                );

                // Export all telemetry messages as json to the Telemetry stream.
                _log?.Telemetry( $"{jsonString}" );
            }

            return ExportResult.Success;
        }

        protected override bool OnShutdown( int timeoutMilliseconds ) {
            _log?.Warn( $"{_name}.OnShutdown(timeoutMilliseconds={timeoutMilliseconds})" );
            return true;
        }

        protected override void Dispose( bool disposing ) { _log?.Debug( $"{_name}.Dispose({disposing})" ); }
    }
}
