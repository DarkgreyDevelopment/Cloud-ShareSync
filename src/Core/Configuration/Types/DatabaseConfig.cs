using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    public class DatabaseConfig {
        public bool UseSqlite { get; set; }
        public string SqliteDBPath { get; set; }
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
