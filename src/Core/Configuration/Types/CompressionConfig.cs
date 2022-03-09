using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    public class CompressionConfig {
        public string DependencyPath { get; set; }

        /// <summary>
        /// Returns the <see cref="CompressionConfig"/> as a json string.
        /// </summary>
        public override string ToString( ) =>
            JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
    }
#nullable enable
}
