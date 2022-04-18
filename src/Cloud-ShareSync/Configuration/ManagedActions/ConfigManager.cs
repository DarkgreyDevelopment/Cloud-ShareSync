using Cloud_ShareSync.Configuration.Interfaces;
using Cloud_ShareSync.Configuration.Types;

namespace Cloud_ShareSync.Configuration.ManagedActions {

    public class ConfigManager {

        public ConfigManager( bool? skipValidBuild = null ) {
            _configPath = ConfigPathHandler.GetConfigPath( skipValidBuild == true );
            ConfigBuilder = new CompleteConfigBuilder( );
            Config = ConfigBuilder.BuildCompleteConfig( _configPath, skipValidBuild == true );
        }

        public string _configPath;

        public readonly CompleteConfig Config;
        public readonly CompleteConfigBuilder ConfigBuilder;

        #region UpdateConfigSection

        internal void UpdateConfigSection( ICloudShareSyncConfig configSection ) {

            UpdateB2ConfigSection( configSection );
            UpdateCompressionConfigSection( configSection );
            UpdateConsoleLogConfigSection( configSection );
            UpdateDatabaseConfigSection( configSection );
            UpdateDefaultLogConfigSection( configSection );
            UpdateLog4NetConfigSection( configSection );
            UpdateSyncConfigSection( configSection );
            UpdateTelemetryLogConfigSection( configSection );

            WriteUpdatedConfig( );
        }

        internal void UpdateB2ConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as B2Config) != null) {
                Config.BackBlaze = (B2Config)configSection;
            }
        }

        internal void UpdateCompressionConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as CompressionConfig) != null) {
                Config.Compression = (CompressionConfig)configSection;
            }
        }

        internal void UpdateConsoleLogConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as ConsoleLogConfig) != null) {
                if (Config.Logging == null) {
                    Config.Logging = new Log4NetConfig( false ) { EnableConsoleLog = true };
                }
                Config.Logging.ConsoleConfiguration = (ConsoleLogConfig)configSection;
            }
        }

        internal void UpdateDatabaseConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as DatabaseConfig) != null) {
                Config.Database = (DatabaseConfig)configSection;
            }
        }

        internal void UpdateDefaultLogConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as DefaultLogConfig) != null) {
                if (Config.Logging == null) {
                    Config.Logging = new Log4NetConfig( false ) { EnableDefaultLog = true };
                }
                Config.Logging.DefaultLogConfiguration = (DefaultLogConfig)configSection;
            }
        }

        internal void UpdateLog4NetConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as Log4NetConfig) != null) {
                Log4NetConfig section = (Log4NetConfig)configSection;
                if (Config.Logging == null) {
                    Config.Logging = section;
                } else {
                    Config.Logging.ConfigurationFile = section.ConfigurationFile;
                    Config.Logging.EnableDefaultLog = section.EnableDefaultLog;
                    Config.Logging.EnableTelemetryLog = section.EnableTelemetryLog;
                    Config.Logging.EnableConsoleLog = section.EnableConsoleLog;
                }
            }
        }

        internal void UpdateSyncConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as SyncConfig) != null) {
                Config.Sync = (SyncConfig)configSection;
            }
        }

        internal void UpdateTelemetryLogConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as TelemetryLogConfig) != null) {
                if (Config.Logging == null) {
                    Config.Logging = new Log4NetConfig( false ) { EnableTelemetryLog = true };
                }
                Config.Logging.TelemetryLogConfiguration = (TelemetryLogConfig)configSection;
            }
        }

        internal void WriteUpdatedConfig( ) {
            Console.WriteLine( $"Writing Cloud-ShareSync config to '{_configPath}'." );
            File.WriteAllText( _configPath, Config.ToString( ) );
        }

        #endregion UpdateConfigSection

    }
}

