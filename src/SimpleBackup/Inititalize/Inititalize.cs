using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.SharedServices;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void Initialize( string[] args ) {

            s_config = Config.GetConfiguration( args );
            ConfigureTelemetryLogger( s_config?.Log4Net );
            s_logger?.ILog?.Info( s_config?.ToString( ) );

            _ = new SystemMemoryChecker( s_logger ); // Configure SystemMemoryChecker
            SystemMemoryChecker.Update( );

            if (s_config == null || s_config.SimpleBackup == null) {
                throw new InvalidDataException( "SimpleBackup configuration required." );
            }
            if (s_config.Database == null) { throw new InvalidDataException( "Database configuration required." ); }
            if (s_config.BackBlaze == null) { throw new InvalidDataException( "Backblaze configuration required." ); }

            if (s_config.SimpleBackup.WorkingDirectory != null && Directory.Exists( s_config.SimpleBackup.WorkingDirectory )) {
                s_logger?.ILog?.Info( "Working Directory Exists" );
                s_fileHash = new( s_logger );
            } else {
                throw new DirectoryNotFoundException(
                    $"Working directory '{s_config?.SimpleBackup?.WorkingDirectory}' doesn't exist." );
            }

            if (s_config.SimpleBackup.CompressBeforeUpload == true && s_config.Compression != null) {
                s_logger?.ILog?.Info( "Inititalizing compression interface." );
                CompressionInterface.Initialize( s_config.Compression, s_logger );
            }

            s_excludePatterns = BuildExcludeRegexArray( s_config.SimpleBackup.ExcludePaths );

            ConfigureDatabase( s_config.Database );

            s_logger?.ILog?.Info( "Inititalizing BackBlaze configuration." );
            s_backBlaze = new( s_config.BackBlaze, s_logger );

            s_uploadProcess = new(
                s_config.SimpleBackup,
                s_config.BackBlaze,
                s_config.Database,
                s_config?.Compression,
                s_logger
            );

            s_logger?.ILog?.Info( "Application Initialized, Begin Processing..." );
        }

    }
}
