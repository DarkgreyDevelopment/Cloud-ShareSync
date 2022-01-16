using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cloud_ShareSync.Core.Logging.Types {
#nullable disable
    public class DefaultLogConfig {
        public string FileName { get; set; }
        public string LogDirectory { get; set; }
        public int RolloverCount { get; set; }
        public int MaximumSize { get; set; }

        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public SupportedLogLevels LogLevels { get; set; }

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
