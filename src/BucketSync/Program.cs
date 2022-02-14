using System.Diagnostics;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.BucketSync.Process;

namespace Cloud_ShareSync.BucketSync {

    public class Program {
        /*
        private static readonly ActivitySource s_source = new( "Cloud_ShareSync.BucketSync.Program" );
        private static TelemetryLogger? s_logger;
        private static CompleteConfig? s_config;
        private static readonly string[] s_sourceList = new string[] {
            "Cloud_ShareSync.BucketSync.Program",
            "B2",
            "BackBlazeB2.PublicInterface",
            "CompressionInterface",
            "Config",
            "FileHash",
            "LocalSyncProcess",
            "ManagedChaCha20Poly1305",
            "MimeType",
            "UniquePassword"
        };
        */

        public static async Task Main( string[] args ) {
            /*
            s_config = Config.GetConfiguration( args );
            ConfigureTelemetryLogger( s_config?.Log4Net );

            using Activity? activity = s_source.StartActivity( "Main" )?.Start( );
            try {
                IHost host = ConfigureHost( args );

                if (s_config?.BackBlaze != null) {
                    s_backBlaze = new( s_config.BackBlaze, s_logger );
                } else {
                    throw new InvalidDataException( "Config is null and backblaze config is required." );
                }

                if (s_config?.SimpleBackup?.CompressBeforeUpload == true && s_config?.Compression != null) {
                    CompressionInterface.Initialize( s_config.Compression, s_logger );
                }

                s_logger?.ILog?.Info( "Configuration Read, Logging Initialized, Begin Processing..." );

                await host.RunAsync( );
            } catch (Exception e) {
                if (s_logger?.ILog == null) {
                    Console.WriteLine( e.ToString( ) );
                } else {
                    s_logger?.ILog?.Fatal( e.ToString( ) );
                }
            }
            activity?.Stop( );
            */
        }

        /*
        private static void ConfigureTelemetryLogger( Log4NetConfig? config ) {
            if (config == null) {
                Console.Error.WriteLine(
                    "Log configuration is null. " +
                    "This likely means that Log4Net was excluded from the Cloud_ShareSync EnabledFeatures. \n" +
                    "Either that or something is catastrophically wrong."
                );
                throw new ArgumentNullException( nameof( config ) );
            }

            s_logger = new( s_sourceList, config );
        }

        private static IHost ConfigureHost( string[] args ) {

            IHostBuilder builder = Host.CreateDefaultBuilder( args )
                                    .ConfigureServices( services => {
                                        services.Configure<BackupConfig>( Config.GetSimpleBackup( ) );
                                        services.AddSingleton<ILocalSyncProcess, LocalSyncProcess>( );
                                        services.AddHostedService<PrimaryWorker>( );
                                    } );

            if (s_logger != null) {
                builder.ConfigureLogging( logging => {
                    logging.ClearProviders( );
                    logging.AddProvider( new Log4NetProvider( (TelemetryLogger)s_logger ) );
                } );
            }

            return builder.Build( );
        }
        */
    }
}
