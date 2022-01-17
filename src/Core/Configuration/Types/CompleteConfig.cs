using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Types.Cloud;
using Cloud_ShareSync.Core.Configuration.Types.Features;
using Cloud_ShareSync.Core.Logging.Types;

namespace Cloud_ShareSync.Core.Configuration.Types {
    public class CompleteConfig {
        public CoreConfig Core { get; set; }
        public BackupConfig? SimpleBackup { get; set; }
        public RestoreConfig? SimpleRestore { get; set; }
        public DatabaseConfig? Database { get; set; }
        public CompressionConfig? Compression { get; set; }
        public B2Config? BackBlaze { get; set; }
        public Log4NetConfig? Log4Net { get; set; }

        public CompleteConfig( CoreConfig general ) { Core = general; }

        public override string ToString( ) {
            JsonSerializerOptions options = new( ) {
                IncludeFields = true,
                WriteIndented = true,
            };
            return JsonSerializer.Serialize( this, options );
        }
    }
}
