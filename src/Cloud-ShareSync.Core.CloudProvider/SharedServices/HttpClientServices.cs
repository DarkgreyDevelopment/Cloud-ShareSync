using Cloud_ShareSync.Core.Logging.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.SharedServices {

    public class HttpClientServices {

        public readonly ServiceProvider Services;

        public HttpClientServices(
            int? uploadThreads,
            ILogger? logger
        ) {
            ServiceCollection services = new( );

            AddHttpClient( services, uploadThreads );

            AddLoggingProvider( services, logger );
            Services = services.BuildServiceProvider( );
        }

        private static void AddHttpClient(
            ServiceCollection services,
            int? uploadThreads
        ) {
            // Register HTTP Clients
            _ = services
                .AddHttpClient<CloudShareSyncHttpClient>( )
                .ConfigurePrimaryHttpMessageHandler(
                    ( ) => new SocketsHttpHandler {
                        MaxConnectionsPerServer = uploadThreads ?? 100
                    }
                ).SetHandlerLifetime( TimeSpan.FromMinutes( 5 ) );
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
