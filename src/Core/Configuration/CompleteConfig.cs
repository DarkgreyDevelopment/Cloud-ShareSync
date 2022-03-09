using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Types;

namespace Cloud_ShareSync.Core.Configuration {
    public class CompleteConfig {
        public CoreConfig Core { get; set; } = new( );
        public BackupConfig? Backup { get; set; } = null;
        public RestoreConfig? Restore { get; set; } = null;
        public DatabaseConfig? Database { get; set; } = new( );
        public Log4NetConfig? Log4Net { get; set; } = new( );
        public CompressionConfig? Compression { get; set; } = null;
        public B2Config? BackBlaze { get; set; } = null;

        /// <summary>
        /// Creating the <see cref="CompleteConfig"/> requires the <see cref="CoreConfig"/> be set at a minimum.
        /// </summary>
        /// <param name="core"></param>
        public CompleteConfig( CoreConfig core ) { Core = core; }

        /// <summary>
        /// Returns the <see cref="CompleteConfig"/> as a json string.
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
}
