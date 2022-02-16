using System.Diagnostics;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.Configuration.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleRestore {

    public partial class Program {
        private static readonly ActivitySource s_source = new( "Cloud_ShareSync.SimpleRestore.Program" );
        private static ILogger? s_logger;
        private static CompleteConfig? s_config;

        public static async Task Main( string[] args ) {
            try {
                s_config = Config.GetConfiguration( args );
                s_logger = Config.ConfigureTelemetryLogger( s_config.Log4Net, Array.Empty<string>( ) );
                using Activity? activity = s_source.StartActivity( "Main" )?.Start( );
                Config.ValidateConfigSet( s_config, s_logger, true, false );

                s_logger?.LogInformation( "Simple restore completed." );
                activity?.Stop( );
            } catch (Exception e) {
                if (s_logger == null) {
                    Console.WriteLine( e.ToString( ) );
                } else {
                    s_logger.LogCritical( "{exception}", e );
                }
            }
        }
    }
}
