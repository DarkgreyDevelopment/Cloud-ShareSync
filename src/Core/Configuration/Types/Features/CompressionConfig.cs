using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types.Features {
#nullable disable
    public class CompressionConfig {
        public string DependencyPath { get; set; }
        public string InterimZipName { get; set; }
        public string InterimZipPath { get; set; }
        public string CompressionCmdlineArgs { get; set; }
        public string DeCompressionCmdlineArgs { get; set; }

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
