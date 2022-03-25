using Cloud_ShareSync.Core.Database;
using Cloud_ShareSync.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.SharedServices {
    internal class CloudShareSyncServices {
        public readonly ServiceProvider Services;

        #region CTor

        public CloudShareSyncServices(
            string? dbPath,
            ILogger? logger
        ) {
            ServiceCollection services = new( );

            AddDbContextFactory( services, dbPath );

            AddLoggingProvider( services, logger );

            Services = services.BuildServiceProvider( );
        }

        public CloudShareSyncServices(
            int? uploadThreads,
            ILogger? logger
        ) {
            ServiceCollection services = new( );

            AddHttpClient( services, uploadThreads );

            AddLoggingProvider( services, logger );

            Services = services.BuildServiceProvider( );
        }

        #endregion CTor


        #region privateMethods

        private static void AddHttpClient(
            ServiceCollection services,
            int? uploadThreads
        ) {
            // Register HTTP Clients
            _ = services
                .AddHttpClient<BackBlazeHttpClient>( )
                .ConfigurePrimaryHttpMessageHandler(
                    ( ) => new SocketsHttpHandler {
                        MaxConnectionsPerServer = uploadThreads ?? 100
                    }
                );
        }

        private static void AddDbContextFactory(
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
                _ = services.AddLogging( loggerBuilder => {
                    _ = loggerBuilder.AddProvider( new Log4NetProvider( logger ) );
                } );
            }
        }

        #endregion privateMethods

        //static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
        //    return HttpPolicyExtensions
        //        .HandleTransientHttpError()
        //        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        //        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        //}
        //
        //static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() {
        //    return HttpPolicyExtensions
        //        .HandleTransientHttpError()
        //        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        //}
    }
}
