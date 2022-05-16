using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Interfaces;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// Cloud-ShareSync database configuration settings.
    /// </summary>
    public class DatabaseConfig : ICloudShareSyncConfig {

        /// <summary>
        /// <para>
        /// When enabled Cloud-ShareSync will store data in a local sqlite database.
        /// </para>
        /// <para>
        /// Cloud-ShareSync requires a database for operation. Either <see cref="UseSqlite"/>
        /// or <see cref="UsePostgres"/> must be true. But only one can be true at any given time.
        /// </para>
        /// </summary>
        /// <value><see langword="true"/></value>
        public bool UseSqlite { get; set; } = true;


        /// <summary>
        /// <para>
        /// The path, either relative or complete, to the directory where the Cloud-ShareSync
        /// sqlite database exists.
        /// </para>
        /// </summary>
        /// <value><seealso cref="AppContext.BaseDirectory"/></value>
        public string SqliteDBPath { get; set; } = AppContext.BaseDirectory;


        /// <summary>
        /// <para>
        /// When enabled Cloud-ShareSync will store data in a non-included postgres database.
        /// When enabled <see cref="PostgresConnectionString"/> must contain a valid postgres 
        /// connection string.
        /// </para>
        /// <para>
        /// Cloud-ShareSync requires a database for operation. Either <see cref="UsePostgres"/>
        /// or <see cref="UseSqlite"/> must be true. But only one can be true at any given time.
        /// </para>
        /// </summary>
        /// <value><see langword="false"/></value>
        public bool UsePostgres { get; set; }


        /// <summary>
        /// The connection string for a postgres database.
        /// </summary>
        /// <value>An empty <see langword="string"/>.</value>
        public string PostgresConnectionString { get; set; } = "";


        /// <value>Returns the <see cref="DatabaseConfig"/> as a json string.</value>
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
