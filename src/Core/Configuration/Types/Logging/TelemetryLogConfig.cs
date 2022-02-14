using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types.Logging {
#nullable disable
    public class TelemetryLogConfig {
        public string FileName { get; set; }
        public string LogDirectory { get; set; }
        public int RolloverCount { get; set; }
        public int MaximumSize { get; set; }

        public override string ToString( ) {
            JsonSerializerOptions options = new( ) {
                IncludeFields = true,
                WriteIndented = true,
            };
            return JsonSerializer.Serialize( this, options );
        }
    }
#nullable enable
}
