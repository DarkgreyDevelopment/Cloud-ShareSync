using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleRestore {

    public partial class Program {
        private static readonly ActivitySource s_source = new( "Cloud_ShareSync.SimpleRestore.Program" );
        private static readonly ILogger? s_logger;

        public static Task Main( string[] args ) {
            try {
                using Activity? activity = s_source.StartActivity( "Main" )?.Start( );

                s_logger?.LogInformation( "Simple restore completed." );
                activity?.Stop( );
            } catch (Exception e) {
                if (s_logger == null) {
                    Console.WriteLine( e.ToString( ) );
                } else {
                    s_logger.LogCritical( "{exception}", e );
                }
            }
            return Task.CompletedTask;
        }
    }
}
