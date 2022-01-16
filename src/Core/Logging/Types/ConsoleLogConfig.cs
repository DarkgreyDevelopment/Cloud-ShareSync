using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cloud_ShareSync.Core.Logging.Types {
#nullable disable
    public class ConsoleLogConfig {

        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public SupportedLogLevels LogLevels { get; set; }
        public bool UseStdErr { get; set; }

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
