using System.Diagnostics;
using Cloud_ShareSync.Core.Configuration;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleRestore {

    public partial class Program {
        private static readonly ActivitySource s_source = new( "Cloud_ShareSync.SimpleRestore.Program" );
        private static ILogger? s_logger;
        private static CompleteConfig? s_config;

        public static async Task Main( string[] args ) {
            try {
                s_config = ConfigManager.GetConfiguration( args );
                s_logger = ConfigManager.ConfigureTelemetryLogger( s_config.Log4Net, Array.Empty<string>( ) );
                using Activity? activity = s_source.StartActivity( "Main" )?.Start( );
                ConfigManager.ValidateConfigSet( s_config, true, false, s_logger );



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
