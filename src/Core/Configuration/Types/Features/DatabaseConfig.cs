using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types.Features {
#nullable disable
    public class DatabaseConfig {
        public bool UseSqllite { get; set; }
        public string SqlliteDBPath { get; set; }
        public bool UsePostgres { get; set; }
        public string PostgresConnectionString { get; set; }

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
