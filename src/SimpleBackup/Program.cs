using System.Diagnostics;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.SimpleBackup.Interfaces;
using Cloud_ShareSync.SimpleBackup.Process;
using Cloud_ShareSync.SimpleBackup.Types;
using Cloud_ShareSync.SimpleBackup.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static ILogger? s_logger;
        private static CompleteConfig? s_config;
        private static Regex[] s_excludePatterns = Array.Empty<Regex>( );
        private static readonly ActivitySource s_source = new( "Cloud_ShareSync.SimpleBackup.Program" );
        private static readonly string[] s_sourceList = Array.Empty<string>( );

        public static async Task Main( string[] args ) {
            try {
                Initialize( args );
                using Activity? activity = s_source.StartActivity( "Main" )?.Start( );
                if (s_config?.SimpleBackup == null) {
                    throw new InvalidOperationException( "Cannot continue if SimpleBackup Config is null." );
                }
                PopulateFileList(
                    PrepUploadFileWorker.Queue,
                    s_excludePatterns,
                    s_config.SimpleBackup,
                    s_logger
                );

                IHost host = ConfigureHost( args );
                Task prepTask = PrepWork( host );
                await UploadWork( host );

                await prepTask;
                await UploadWork( host );

                //await host.RunAsync( );
                s_logger?.LogInformation( "Simple backup completed." );
                activity?.Stop( );
            } catch (Exception e) {
                if (s_logger == null) {
                    Console.WriteLine( e.ToString( ) );
                } else {
                    s_logger.LogCritical( "{exception}", e );
                }
            }
        }

        private static async Task PrepWork( IHost host ) {
            IPrepUploadFileProcess prepWorker = host.Services.GetRequiredService<IPrepUploadFileProcess>( );
            await prepWorker.Prep( PrepUploadFileWorker.Queue );
        }

        private static async Task UploadWork( IHost host ) {
            IUploadFileProcess uploadWorker = host.Services.GetRequiredService<IUploadFileProcess>( );
            s_logger?.LogDebug( "Upload Work." );

            if (PrepUploadFileWorker.Queue.IsEmpty == false && UploadFileWorker.Queue.IsEmpty) {
                Thread.Sleep( 5000 );
            }

            while (UploadFileWorker.Queue.IsEmpty == false) {
                s_logger?.LogDebug( "Upload Work Process." );
                bool deQueue = UploadFileWorker.Queue.TryDequeue( out UploadFileInput? ufInput );
                if (deQueue && ufInput != null) {
                    await uploadWorker.Process( ufInput );
                }
            }
        }

        private static IHost ConfigureHost( string[] args ) {

            if (s_config?.SimpleBackup == null || s_config?.BackBlaze == null || s_config?.Database == null) {
                throw new InvalidOperationException( );
            }
            IHostBuilder builder = Host.CreateDefaultBuilder( args )
                                    .ConfigureServices( services => {
                                        services.Configure<BackupConfig>( Config.GetSimpleBackup( ) );
                                        services.AddSingleton( _ => s_config.SimpleBackup );
                                        services.Configure<B2Config>( Config.GetBackBlazeB2( ) );
                                        services.AddSingleton( _ => s_config.BackBlaze );
                                        services.Configure<DatabaseConfig>( Config.GetDatabase( ) );
                                        services.AddSingleton( _ => s_config.Database );
                                        services.Configure<CompressionConfig?>( Config.GetCompression( ) );
                                        if (s_config.Compression != null) _ = services.AddSingleton( _ => s_config.Compression );
                                        services.AddSingleton<IPrepUploadFileProcess, PrepUploadFileProcess>( );
                                        services.AddSingleton<IUploadFileProcess, UploadFileProcess>( );
                                        services.AddHostedService<PrepUploadFileWorker>( );
                                        services.AddHostedService<UploadFileWorker>( );
                                    } );

            if (s_logger != null) {
                s_logger.LogInformation( "Configuring host logging." );

                builder.ConfigureLogging( logging => {
                    logging.ClearProviders( );
                    logging.SetMinimumLevel( GetMinimumLogLevel( ) );
                    logging.AddProvider( new Log4NetProvider( s_logger ) );
                } );
            }

            return builder.Build( );
        }

        private static LogLevel GetMinimumLogLevel( ) {
            LogLevel lvl = LogLevel.None;
            if (s_logger != null) {
                if (s_logger.IsEnabled( LogLevel.Trace )) {
                    lvl = LogLevel.Trace;
                } else if (s_logger.IsEnabled( LogLevel.Debug )) {
                    lvl = LogLevel.Debug;
                } else if (s_logger.IsEnabled( LogLevel.Information )) {
                    lvl = LogLevel.Information;
                } else if (s_logger.IsEnabled( LogLevel.Warning )) {
                    lvl = LogLevel.Warning;
                } else if (s_logger.IsEnabled( LogLevel.Error )) {
                    lvl = LogLevel.Error;
                } else if (s_logger.IsEnabled( LogLevel.Critical )) {
                    lvl = LogLevel.Critical;
                }
            }
            return lvl;
        }
    }
}
