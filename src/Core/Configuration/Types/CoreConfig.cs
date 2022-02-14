using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Configuration.Enums;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    public class CoreConfig {

        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public Cloud_ShareSync_Features EnabledFeatures { get; set; }

        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public CloudProviders EnabledCloudProviders { get; set; }

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
