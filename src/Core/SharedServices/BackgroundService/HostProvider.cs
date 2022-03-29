using System.Diagnostics;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.Core.SharedServices.BackgroundService.Interfaces;
using Cloud_ShareSync.Core.SharedServices.BackgroundService.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.SharedServices.BackgroundService {
    public class HostProvider {
        private static readonly ActivitySource s_source = new( "HostProvider" );

        public static IHost ConfigureHost( ILogger? log, CompleteConfig config, ConfigManager cfgMgr ) {
            using Activity? activity = s_source.StartActivity( "ConfigureHost" )?.Start( );
            if (config?.Database == null) {
                throw new ApplicationException( );
            }
            IHostBuilder builder = Host.CreateDefaultBuilder( Array.Empty<string>( ) )
                                    .ConfigureServices( services => {
                                        _ = services.Configure<DatabaseConfig>( cfgMgr.GetDatabase( ) );
                                        _ = services.AddSingleton( _ => config.Database );
                                        _ = services.Configure<CompressionConfig?>( cfgMgr.GetCompression( ) );
                                        _ = services.AddSingleton( _ => config.Compression ?? new( ) { DependencyPath = "" } );
                                        if (config.BackBlaze != null) {
                                            _ = services.Configure<B2Config>( cfgMgr.GetBackBlazeB2( ) );
                                            _ = services.AddSingleton( _ => config.BackBlaze );
                                        }
                                        if (config.Sync != null) {
                                            _ = services.Configure<SyncConfig>( cfgMgr.GetSyncConfig( ) );
                                            _ = services.AddSingleton( _ => config.Sync );
                                            _ = services.AddSingleton<IPrepUploadFileProcess, PrepUploadFileProcess>( );
                                            _ = services.AddSingleton<IUploadFileProcess, UploadFileProcess>( );
                                        }
                                    } );

            if (log != null) {
                log.LogInformation( "Configuring host logging." );

                _ = builder.ConfigureLogging( logging => {
                    _ = logging.ClearProviders( );
                    _ = logging.SetMinimumLevel( GetMinimumLogLevel( log ) );
                    _ = logging.AddProvider( new Log4NetProvider( log ) );
                } );
            }
            IHost host = builder.Build( );

            activity?.Stop( );
            return host;
        }

        private static LogLevel GetMinimumLogLevel( ILogger log ) {
            LogLevel lvl = LogLevel.None;
            if (log.IsEnabled( LogLevel.Trace )) {
                lvl = LogLevel.Trace;
            } else if (log.IsEnabled( LogLevel.Debug )) {
                lvl = LogLevel.Debug;
            } else if (log.IsEnabled( LogLevel.Information )) {
                lvl = LogLevel.Information;
            } else if (log.IsEnabled( LogLevel.Warning )) {
                lvl = LogLevel.Warning;
            } else if (log.IsEnabled( LogLevel.Error )) {
                lvl = LogLevel.Error;
            } else if (log.IsEnabled( LogLevel.Critical )) {
                lvl = LogLevel.Critical;
            }
            return lvl;
        }

    }
}
