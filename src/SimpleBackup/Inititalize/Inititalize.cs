using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void Initialize( string[] args ) {

            s_config = Config.GetConfiguration( args );
            ConfigureTelemetryLogger( s_config?.Log4Net );
            s_logger?.LogInformation( "{string}", s_config?.ToString( ) );

            SystemMemoryChecker.Inititalize( s_logger ); // Configure SystemMemoryChecker
            SystemMemoryChecker.Update( );

            if (s_config == null || s_config.SimpleBackup == null) {
                throw new InvalidDataException( "SimpleBackup configuration required." );
            }
            if (s_config.Database == null) { throw new InvalidDataException( "Database configuration required." ); }
            if (s_config.BackBlaze == null) { throw new InvalidDataException( "Backblaze configuration required." ); }

            if (s_config.SimpleBackup.WorkingDirectory != null && Directory.Exists( s_config.SimpleBackup.WorkingDirectory )) {
                s_logger?.LogInformation( "Working Directory Exists" );
            } else {
                throw new DirectoryNotFoundException(
                    $"Working directory '{s_config?.SimpleBackup?.WorkingDirectory}' doesn't exist." );
            }

            s_excludePatterns = BuildExcludeRegexArray( s_config.SimpleBackup.ExcludePaths );

            ConfigureDatabase( s_config.Database );

            s_logger?.LogInformation( "Application Initialized, Begin Processing..." );
        }

    }
}
