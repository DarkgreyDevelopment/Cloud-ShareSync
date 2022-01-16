using System.Diagnostics;
using System.Text.Json;
using log4net;
using OpenTelemetry;

namespace Cloud_ShareSync.Core.Logging.Types {
    internal class LogExporter : BaseExporter<Activity> {

        private readonly string _name;
        private readonly ILog? _log;

        internal LogExporter( string name = "LogExporter" ) { this._name = name; }

        internal LogExporter(
            ILog? log,
            string name = "LogExporter"
        ) {
            _name = name;
            _log = log;
        }

        public override ExportResult Export( in Batch<Activity> batch ) {

            using IDisposable scope = SuppressInstrumentationScope.Begin( );
            foreach (Activity activity in batch) {
                JsonSerializerOptions jsonOptions = new( ) { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize( activity, jsonOptions );

                // Export all telemetry messages as json strings to the Telemetry stream.
                _log?.Telemetry( $"\n{jsonString}" );
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
