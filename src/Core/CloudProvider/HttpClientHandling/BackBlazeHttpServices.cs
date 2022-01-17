using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.Core.Logging.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.HttpClientHandling {
    public class BackBlazeHttpServices {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static ServiceProvider Services { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public BackBlazeHttpServices(
            int uploadThreads,
            TelemetryLogger? logger
        ) {
            ServiceCollection services = new( );

            // Register a HTTP Clients
            services
                .AddHttpClient<BackBlazeHttpClient>( )
                .ConfigurePrimaryHttpMessageHandler(
                    ( ) => new SocketsHttpHandler {
                        MaxConnectionsPerServer = uploadThreads
                    }
                );
            if (logger != null)
                services.AddLogging(
                    loggerBuilder => { loggerBuilder.AddProvider( new Log4NetProvider( logger ) ); }
                );

            Services = services.BuildServiceProvider( );
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
