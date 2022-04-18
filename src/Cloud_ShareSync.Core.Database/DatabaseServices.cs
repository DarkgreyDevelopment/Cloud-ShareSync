using Cloud_ShareSync.Core.Logging.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Database {
    public class DatabaseServices {

        public DatabaseServices(
            string? dbPath,
            ILogger? logger
        ) {
            ServiceCollection services = new( );

            AddSqliteContextToDbContextFactory( services, dbPath );

            AddLoggingProvider( services, logger );

            Services = services.BuildServiceProvider( );
            LogTableCounts( logger );
        }

        public readonly ServiceProvider Services;

        private static void AddSqliteContextToDbContextFactory(
            ServiceCollection services,
            string? dbPath
        ) {
            // Register dbContext
            string sqliteDBSource = SqliteContext.DetermineDbPath( dbPath ?? "" );
            SqliteContext.DatabasePath = sqliteDBSource;
            _ = services.AddDbContextFactory<SqliteContext>( options =>
                  options.UseSqlite( $"Data Source={sqliteDBSource}" )
            );
        }

        private static void AddLoggingProvider(
            ServiceCollection services,
            ILogger? logger
        ) {
            if (logger != null) {
                _ = services.AddLogging(
                    loggerBuilder => {
                        _ = loggerBuilder.AddProvider( new Log4NetProvider( logger ) );
                    }
                );
            }
        }

        private void LogTableCounts( ILogger? log ) {
            SqliteContext sqliteContext = Services.GetRequiredService<SqliteContext>( );
            log?.LogInformation( "Database Service Initialized." );
            LogCoreTableCount( sqliteContext, log );
            LogEncryptedTableCount( sqliteContext, log );
            LogCompressedTableCount( sqliteContext, log );
            LogBackBlazeTableCount( sqliteContext, log );
        }

        private static void LogCoreTableCount( SqliteContext sqliteContext, ILogger? log ) {
            log?.LogInformation(
                "Core Table      : {string}",
                (from obj in sqliteContext.CoreData where obj.Id >= 0 select obj).Count( )
            );
        }

        private static void LogEncryptedTableCount( SqliteContext sqliteContext, ILogger? log ) {
            log?.LogInformation(
                "Encrypted Table : {string}",
                (from obj in sqliteContext.EncryptionData where obj.Id >= 0 select obj).Count( )
            );
        }

        private static void LogCompressedTableCount( SqliteContext sqliteContext, ILogger? log ) {
            log?.LogInformation(
                "Compressed Table: {string}",
                (from obj in sqliteContext.CompressionData where obj.Id >= 0 select obj).Count( )
            );
        }

        private static void LogBackBlazeTableCount( SqliteContext sqliteContext, ILogger? log ) {
            log?.LogInformation(
                "BackBlaze Table : {string}",
                (from obj in sqliteContext.BackBlazeB2Data where obj.Id >= 0 select obj).Count( )
            );
        }
    }
}
