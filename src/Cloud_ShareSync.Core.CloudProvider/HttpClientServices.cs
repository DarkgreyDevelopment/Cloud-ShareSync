using Cloud_ShareSync.Core.Logging.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.SharedServices {

    internal class HttpClientServices {

        public readonly ServiceProvider Services;

        public HttpClientServices(
            int? uploadThreads,
            ILogger? logger
        ) {
            ServiceCollection services = new( );

            AddB2HttpClient( services, uploadThreads );

            AddLoggingProvider( services, logger );

            Services = services.BuildServiceProvider( );
        }


        private static void AddB2HttpClient(
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

    }
}
