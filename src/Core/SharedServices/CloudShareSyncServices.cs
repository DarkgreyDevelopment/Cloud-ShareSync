﻿using Cloud_ShareSync.Core.Database.Sqlite;
using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.Core.Logging.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.SharedServices {
    public class CloudShareSyncServices {
        public readonly ServiceProvider Services;

        public CloudShareSyncServices(
            string? dbPath,
            TelemetryLogger? logger
        ) {
            ServiceCollection services = new( );

            AddDbContextFactory( services, dbPath );

            AddLoggingProvider( services, logger );

            Services = services.BuildServiceProvider( );
        }

        public CloudShareSyncServices(
            int? uploadThreads,
            TelemetryLogger? logger
        ) {
            ServiceCollection services = new( );

            AddHttpClient( services, uploadThreads );

            AddLoggingProvider( services, logger );

            Services = services.BuildServiceProvider( );
        }

        public CloudShareSyncServices(
            string? dbPath,
            int? uploadThreads,
            TelemetryLogger? logger
        ) {
            ServiceCollection services = new( );

            AddDbContextFactory( services, dbPath );

            AddHttpClient( services, uploadThreads );

            AddLoggingProvider( services, logger );

            Services = services.BuildServiceProvider( );
        }

        private static void AddHttpClient(
            ServiceCollection services,
            int? uploadThreads
        ) {
            // Register HTTP Clients
            services
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
            string sqliteDBSource = SqliteContext.DetermineDbPath( dbPath );
            SqliteContext.DatabasePath = sqliteDBSource;
            services.AddDbContextFactory<SqliteContext>( options =>
                options.UseSqlite( $"Data Source={sqliteDBSource}" )
            );
        }

        private static void AddLoggingProvider(
            ServiceCollection services,
            TelemetryLogger? logger
        ) {
            if (logger != null)
                services.AddLogging( loggerBuilder => {
                    loggerBuilder.AddProvider( new Log4NetProvider( logger ) );
                } );
        }

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