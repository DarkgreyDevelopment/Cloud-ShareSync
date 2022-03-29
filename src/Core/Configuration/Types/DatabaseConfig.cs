using System.CommandLine;
using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// Cloud-ShareSync database configuration settings.
    /// </summary>
    public class DatabaseConfig {

        #region UseSqlite

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

        private static Option<bool> NewUseSqliteOption( Command verbCommand ) {
            Option<bool> sqliteOption = new(
                name: "--UseSqlite",
                description: "When enabled Cloud-ShareSync will store data in a local sqlite database.",
                getDefaultValue: ( ) => true
            );
            sqliteOption.AddAlias( "-s" );

            verbCommand.AddOption( sqliteOption );

            return sqliteOption;
        }

        #endregion UseSqlite


        #region SqliteDBPath

        /// <summary>
        /// <para>
        /// The path, either relative or complete, to the directory where the Cloud-ShareSync
        /// sqlite database exists.
        /// </para>
        /// </summary>
        /// <value><seealso cref="AppContext.BaseDirectory"/></value>
        public string SqliteDBPath { get; set; } = AppContext.BaseDirectory;

        private static Option<string> NewSqliteDBPathOption( Command verbCommand ) {
            Option<string> sqliteDBPathOption = new(
                name: "--SqliteDBPath",
                description: "Specify the sqlite database path. This can be the path to a file or directory.",
                getDefaultValue: ( ) => AppContext.BaseDirectory
            );
            sqliteDBPathOption.AddAlias( "-d" );

            verbCommand.AddOption( sqliteDBPathOption );

            return sqliteDBPathOption;
        }

        #endregion SqliteDBPath


        #region UsePostgres

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

        private static Option<bool> NewUsePostgresOption( Command verbCommand ) {
            Option<bool> postgresOption = new(
                name: "--UsePostgres",
                description: "When enabled Cloud-ShareSync will store data in a remote postgres database.",
                getDefaultValue: ( ) => false
            );
            postgresOption.AddAlias( "-p" );

            verbCommand.AddOption( postgresOption );

            return postgresOption;
        }

        #endregion UsePostgres


        #region PostgresConnectionString

        /// <summary>
        /// The connection string for a postgres database.
        /// </summary>
        /// <value>An empty <see langword="string"/>.</value>
        public string PostgresConnectionString { get; set; } = "";

        private static Option<string> NewPostgresConnectionStringOption( Command verbCommand ) {
            Option<string> postgresConnectionStringOption = new(
                name: "--PostgresConnectionString",
                description: "Specify the connection string to the postgres database.",
                getDefaultValue: ( ) => string.Empty
            );
            postgresConnectionStringOption.AddAlias( "-c" );

            verbCommand.AddOption( postgresConnectionStringOption );

            return postgresConnectionStringOption;
        }

        #endregion PostgresConnectionString


        #region VerbHandling

        public static Command NewDatabaseConfigCommand( Option<FileInfo> configPath ) {
            Command databaseConfig = new( "Database" );
            databaseConfig.AddAlias( "database" );
            databaseConfig.AddAlias( "db" );
            databaseConfig.Description = "Edit the Cloud-ShareSync database config.";

            SetDatabaseConfigHandler(
                databaseConfig,
                NewUseSqliteOption( databaseConfig ),
                NewSqliteDBPathOption( databaseConfig ),
                NewUsePostgresOption( databaseConfig ),
                NewPostgresConnectionStringOption( databaseConfig ),
                configPath
            );
            return databaseConfig;
        }

        internal static void SetDatabaseConfigHandler(
            Command databaseConfig,
            Option<bool> useSqlite,
            Option<string> sqliteDBPath,
            Option<bool> usePostgres,
            Option<string> postgresConnectionString,
            Option<FileInfo> configPath
        ) {
            databaseConfig.SetHandler( (
                     bool useSqlite,
                     string sqliteDBPath,
                     bool usePostgres,
                     string postgresConnectionString,
                     FileInfo configPath
                ) => {
                    if (configPath != null) { ConfigManager.SetAltDefaultConfigPath( configPath.FullName ); }

                    DatabaseConfig config = new( ) {
                        UseSqlite = useSqlite,
                        SqliteDBPath = sqliteDBPath,
                        UsePostgres = usePostgres,
                        PostgresConnectionString = postgresConnectionString

                    };
                    Console.WriteLine( $"{config}" );
                },
                useSqlite,
                sqliteDBPath,
                usePostgres,
                postgresConnectionString,
                configPath
            );
        }

        #endregion VerbHandling


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
