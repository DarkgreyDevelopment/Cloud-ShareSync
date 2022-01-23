using Cloud_ShareSync.Core.CloudProvider.BackBlaze;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Configuration;

namespace Cloud_ShareSync.SimpleRestore {

    public partial class Program {

        private static void Initialize( string[] args ) {

            s_config = Config.GetConfiguration( args );
            ConfigureTelemetryLogger( s_config?.Log4Net );
            s_logger?.ILog?.Info( s_config?.ToString( ) );

            if (s_config?.SimpleRestore == null) {
                throw new InvalidDataException( "SimpleRestore configuration required." );
            }

            if (s_config.SimpleRestore.WorkingDirectory != null && Directory.Exists( s_config.SimpleRestore.WorkingDirectory )) {
                s_logger?.ILog?.Info( "Working Directory Exists" );
            } else {
                throw new DirectoryNotFoundException(
                    $"Working directory '{s_config?.SimpleRestore?.WorkingDirectory}' doesn't exist." );
            }

            if (s_config.SimpleRestore.RootFolder != null && Directory.Exists( s_config.SimpleRestore.RootFolder )) {
                s_logger?.ILog?.Info( "Root Directory Exists" );
            } else {
                throw new DirectoryNotFoundException(
                    $"Root directory '{s_config?.SimpleRestore?.RootFolder}' doesn't exist." );
            }

            ConfigureDatabase( );

            if (s_config?.Compression != null) {
                CompressionInterface.Initialize( s_config.Compression, s_logger );
            }

            if (s_config?.BackBlaze != null) {
                s_backBlaze = new( s_config.BackBlaze, s_logger );
            } else {
                throw new InvalidDataException( "Backblaze configuration required." );
            }

            s_logger?.ILog?.Info( "Configuration Read, Logging Initialized, Begin Processing..." );
        }

    }
}
