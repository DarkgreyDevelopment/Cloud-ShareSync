using Cloud_ShareSync.Core.CloudProvider.BackBlaze;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Configuration;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void Inititalize( string[] args ) {

            s_config = Config.GetConfiguration( args );
            ConfigureTelemetryLogger( s_config?.Log4Net );
            s_logger?.ILog?.Info( s_config?.ToString( ) );

            if (s_config?.BucketSync == null) {
                throw new InvalidDataException( "BucketSync configuration required." );
            }

            if (s_config.BucketSync.WorkingDirectory != null && Directory.Exists( s_config.BucketSync.WorkingDirectory )) {
                s_logger?.ILog?.Info( "Working Directory Exists" );
            } else {
                throw new DirectoryNotFoundException(
                    $"Working directory '{s_config?.BucketSync?.WorkingDirectory}' doesn't exist." );
            }

            s_excludePatterns = BuildExcludeRegexArray( s_config.BucketSync.ExcludePaths );

            ConfigureDatabase( );

            if (s_config?.BackBlaze != null) {
                BackBlazeB2.Initialize( s_config.BackBlaze, s_logger );
            } else {
                throw new InvalidDataException( "Backblaze configuration required." );
            }

            if (s_config?.BucketSync?.CompressBeforeUpload == true && s_config?.Compression != null) {
                CompressionInterface.Initialize( s_config.Compression, s_logger );
            }

            s_logger?.ILog?.Info( "Configuration Read, Logging Inititalized, Begin Processing..." );
        }

    }
}
