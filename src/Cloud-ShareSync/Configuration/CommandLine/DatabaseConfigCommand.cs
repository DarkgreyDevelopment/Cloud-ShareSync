using System.CommandLine;
using Cloud_ShareSync.Configuration.ManagedActions;
using Cloud_ShareSync.Configuration.Types;

namespace Cloud_ShareSync.Configuration.CommandLine {
#nullable disable

    public class DatabaseConfigCommand : Command {

        public DatabaseConfigCommand( Option<FileInfo> configPath ) : base(
            name: "Database",
            description: "Edit the Cloud-ShareSync database config."
        ) {
            SetUseSqliteOptionAlias( );
            AddOption( _useSqliteOption );

            SetSqliteDBPathOptionAlias( );
            AddOption( _sqliteDBPathOption );

            SetUsePostgresOptionAlias( );
            AddOption( _usePostgresOption );

            SetPostgresConnectionStringOptionAlias( );
            AddOption( _postgresConnectionStringOption );

            AddAlias( "database" );
            AddAlias( "db" );

            SetDatabaseConfigCommandHandler( configPath );
        }


        private readonly Option<bool> _useSqliteOption = new(
                name: "--UseSqlite",
                description: "When enabled Cloud-ShareSync will store data in a local sqlite database.",
                getDefaultValue: ( ) => true
            ) {
            IsRequired = false
        };

        private void SetUseSqliteOptionAlias( ) { _useSqliteOption.AddAlias( "-s" ); }


        private readonly Option<string> _sqliteDBPathOption = new(
                name: "--SqliteDBPath",
                description: "Specify the sqlite database path. This can be the path to a file or directory.",
                getDefaultValue: ( ) => AppContext.BaseDirectory
            ) {
            IsRequired = false
        };

        private void SetSqliteDBPathOptionAlias( ) { _sqliteDBPathOption.AddAlias( "-d" ); }


        private readonly Option<bool> _usePostgresOption = new(
                name: "--UsePostgres",
                description: "When enabled Cloud-ShareSync will store data in a remote postgres database.",
                getDefaultValue: ( ) => false
            ) {
            IsRequired = false
        };

        private void SetUsePostgresOptionAlias( ) { _usePostgresOption.AddAlias( "-p" ); }


        private readonly Option<string> _postgresConnectionStringOption = new(
                name: "--PostgresConnectionString",
                description: "Specify the connection string to the postgres database.",
                getDefaultValue: ( ) => string.Empty
            ) {
            IsRequired = false
        };

        private void SetPostgresConnectionStringOptionAlias( ) { _postgresConnectionStringOption.AddAlias( "-c" ); }

        private void SetDatabaseConfigCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler(
                (
                     bool useSqlite,
                     string sqliteDBPath,
                     bool usePostgres,
                     string postgresConnectionString,
                     FileInfo configPath
                ) => {
                    if (configPath != null) { ConfigPathHandler.SetAltDefaultConfigPath( configPath.FullName ); }

                    DatabaseConfig config = new( ) {
                        UseSqlite = useSqlite,
                        SqliteDBPath = sqliteDBPath,
                        UsePostgres = usePostgres,
                        PostgresConnectionString = postgresConnectionString

                    };
                    new ConfigManager( ).UpdateConfigSection( config );
                },
                _useSqliteOption,
                _sqliteDBPathOption,
                _usePostgresOption,
                _postgresConnectionStringOption,
                configPath
            );
        }

    }
#nullable enable
}
