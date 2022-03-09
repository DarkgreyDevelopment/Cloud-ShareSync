using System.Reflection;
using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// Cloud-ShareSync database configuration settings.
    /// </summary>
    public class DatabaseConfig {
        private static readonly string s_assemblyPath = Directory.GetParent(
                                        Assembly.GetExecutingAssembly( ).Location
                                     )?.FullName ?? "";
        /// <summary>
        /// <para>
        /// When enabled Cloud-ShareSync will store data in a local sqlite database.
        /// </para>
        /// <para>
        /// Cloud-ShareSync requires a database for operation. Either <see cref="UseSqlite"/>
        /// or <see cref="UsePostgres"/> must be true. But only one can be true at any given time.
        /// </para>
        /// </summary>
        /// <value>Defaults to true.</value>
        public bool UseSqlite { get; set; } = true;

        /// <summary>
        /// <para>
        /// The path, either relative or complete, to the directory where the Cloud-ShareSync
        /// sqlite database exists.
        /// </para>
        /// </summary>
        /// <value>Default value is the application root directory.</value>
        public string SqliteDBPath { get; set; } = s_assemblyPath;

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
        /// <value>Defaults to false.</value>
        public bool UsePostgres { get; set; } = false;

        /// <summary>
        /// The connection string for a postgres database.
        /// </summary>
        /// <value>Defaults to an empty string.</value>
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
