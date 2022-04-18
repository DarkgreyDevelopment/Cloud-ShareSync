using System.Text.Json;
using Cloud_ShareSync.Configuration.Interfaces;

namespace Cloud_ShareSync.Configuration.Types {
#nullable disable
    /// <summary>
    /// Configuration settings to use when compression has been enabled.
    /// </summary>
    public class CompressionConfig : ICloudShareSyncConfig {

        /// <summary>
        /// Specify the path to the 7zip executable.
        /// </summary>
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
