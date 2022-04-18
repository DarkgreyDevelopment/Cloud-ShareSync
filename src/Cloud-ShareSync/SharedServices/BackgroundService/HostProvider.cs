using System.Diagnostics;
using Cloud_ShareSync.Configuration.ManagedActions;
using Cloud_ShareSync.Configuration.Types;
using Cloud_ShareSync.Core.Logging.Logger;
using Cloud_ShareSync.SharedServices.BackgroundService.Interfaces;
using Cloud_ShareSync.SharedServices.BackgroundService.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SharedServices.BackgroundService {
    public class HostProvider {
        private static readonly ActivitySource s_source = new( "HostProvider" );

        public static IHost ConfigureHost( ILogger? log, ConfigManager cfgMgr ) {
            using Activity? activity = s_source.StartActivity( "ConfigureHost" )?.Start( );
            IHostBuilder builder = Host.CreateDefaultBuilder( Array.Empty<string>( ) )
                                    .ConfigureServices( services => {
                                        ConfigureDatabaseService( services, cfgMgr );
                                        ConfigureCompressionService( services, cfgMgr );
                                        ConfigureBackBlazeService( services, cfgMgr );
                                        ConfigureSyncService( services, cfgMgr );
                                    } );
            ConfigureHostLogging( builder, log );
            IHost host = builder.Build( );

            activity?.Stop( );
            return host;
        }

        private static void ConfigureDatabaseService( IServiceCollection services, ConfigManager cfgMgr ) {
            _ = services.Configure<DatabaseConfig>( cfgMgr.ConfigBuilder.GetDatabaseConfigSection( ) );
            _ = services.AddSingleton( _ => cfgMgr.Config.Database );
        }

        private static void ConfigureCompressionService( IServiceCollection services, ConfigManager cfgMgr ) {
            _ = services.Configure<CompressionConfig?>( cfgMgr.ConfigBuilder.GetCompressionConfigSection( ) );
            _ = services.AddSingleton( _ => cfgMgr.Config.Compression ?? new( ) { DependencyPath = "" } );
        }

        private static void ConfigureBackBlazeService( IServiceCollection services, ConfigManager cfgMgr ) {
            if (cfgMgr.Config.BackBlaze != null) {
                _ = services.Configure<B2Config>( cfgMgr.ConfigBuilder.GetB2ConfigSection( ) );
                _ = services.AddSingleton( _ => cfgMgr.Config.BackBlaze );
            }
        }

        private static void ConfigureSyncService( IServiceCollection services, ConfigManager cfgMgr ) {
            if (cfgMgr.Config.Sync != null) {
                _ = services.Configure<SyncConfig>( cfgMgr.ConfigBuilder.GetSyncConfigSection( ) );
                _ = services.AddSingleton( _ => cfgMgr.Config.Sync );
                _ = services.AddSingleton<IPrepUploadFileProcess, PrepUploadFileProcess>( );
                _ = services.AddSingleton<IUploadFileProcess, UploadFileProcess>( );
            }
        }

        private static void ConfigureHostLogging( IHostBuilder builder, ILogger? log ) {
            if (log != null) {
                log.LogInformation( "Configuring host logging." );

                _ = builder.ConfigureLogging(
                    logging => {
                        _ = logging.ClearProviders( );
                        _ = logging.SetMinimumLevel( GetMinimumLogLevel( log ) );
                        _ = logging.AddProvider( new Log4NetProvider( log ) );
                    }
                );
            }
        }

        private static LogLevel GetMinimumLogLevel( ILogger log ) =>
            true switch {
                true when log.IsEnabled( LogLevel.Trace ) => LogLevel.Trace,
                true when log.IsEnabled( LogLevel.Debug ) => LogLevel.Debug,
                true when log.IsEnabled( LogLevel.Information ) => LogLevel.Information,
                true when log.IsEnabled( LogLevel.Warning ) => LogLevel.Warning,
                true when log.IsEnabled( LogLevel.Error ) => LogLevel.Error,
                true when log.IsEnabled( LogLevel.Critical ) => LogLevel.Critical,
                _ => LogLevel.None,
            };


    }
}
