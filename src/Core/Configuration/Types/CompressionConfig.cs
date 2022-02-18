using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    public class CompressionConfig {
        public string DependencyPath { get; set; }

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
