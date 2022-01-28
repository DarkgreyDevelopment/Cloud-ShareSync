using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.SharedServices;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void Initialize( string[] args ) {

            s_config = Config.GetConfiguration( args );
            ConfigureTelemetryLogger( s_config?.Log4Net );
            s_logger?.ILog?.Info( s_config?.ToString( ) );

            _ = new MemoryChecker( s_logger ); // Configure MemoryChecker
            MemoryChecker.Update( );

            if (s_config?.SimpleBackup == null) {
                throw new InvalidDataException( "SimpleBackup configuration required." );
            }

            if (s_config.SimpleBackup.WorkingDirectory != null && Directory.Exists( s_config.SimpleBackup.WorkingDirectory )) {
                s_logger?.ILog?.Info( "Working Directory Exists" );
                if (s_fileHash == null) {
                    s_logger?.ILog?.Info( "Initializing filehash." );
                    s_fileHash = new( s_logger?.ILog );
                }
            } else {
                throw new DirectoryNotFoundException(
                    $"Working directory '{s_config?.SimpleBackup?.WorkingDirectory}' doesn't exist." );
            }

            s_excludePatterns = BuildExcludeRegexArray( s_config.SimpleBackup.ExcludePaths );

            ConfigureDatabase( );

            if (s_config?.BackBlaze != null) {
                s_logger?.ILog?.Info( "Inititalizing BackBlaze configuration." );
                s_backBlaze = new( s_config.BackBlaze, s_logger );
            } else {
                throw new InvalidDataException( "Backblaze configuration required." );
            }

            if (s_config?.SimpleBackup?.CompressBeforeUpload == true && s_config?.Compression != null) {
                s_logger?.ILog?.Info( "Inititalizing compression interface." );
                CompressionInterface.Initialize( s_config.Compression, s_logger );
            }

            s_logger?.ILog?.Info( "Application Initialized, Begin Processing..." );
        }

    }
}
