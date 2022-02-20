using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Types;

namespace Cloud_ShareSync.Core.Configuration {
    public class CompleteConfig {
        public CoreConfig Core { get; set; }
        public BackupConfig? Backup { get; set; }
        public RestoreConfig? Restore { get; set; }
        public DatabaseConfig? Database { get; set; }
        public Log4NetConfig? Log4Net { get; set; }
        public CompressionConfig? Compression { get; set; }
        public S3Config? Aws { get; set; }
        public AzConfig? Azure { get; set; }
        public B2Config? BackBlaze { get; set; }
        public GcsConfig? Google { get; set; }

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
