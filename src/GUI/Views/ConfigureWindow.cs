using Avalonia.Controls;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.Configuration.Interfaces;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.GUI.Types;
using Avalonia.Layout;

namespace Cloud_ShareSync.GUI.Views {
    public partial class ConfigureWindow : Window {

        public ConfigureWindow( ) {
            SetTabContent( );
            SizeToContent = SizeToContent.WidthAndHeight;
            Icon = App.Icon;

            var mainTab = new TabControl {
                Items = new List<TabItem>( ) {
                    _main,
                    _sync,
                    _database,
                    _logging,
                    _compression,
                    _backBlaze,
                },
                TabStripPlacement = Dock.Left
            };
            var logTab = new TabControl {
                Items = new List<TabItem>( ) {
                    _defaultLogging,
                    _telemetryLogging,
                    _consoleLogging
                },
                TabStripPlacement = Dock.Left
            };
            _logging.Content = logTab;

            Content = mainTab;
        }

        #region Fields

        internal ConfigManager CfgMgr { get; private set; } = new ConfigManager( true );

        private readonly TabItem _main = new( ) {
            Name = "Main",
            Header = "Main",
            VerticalContentAlignment = VerticalAlignment.Center
        };

        private readonly TabItem _sync = new( ) {
            Name = "Sync",
            Header = "Sync",
            VerticalContentAlignment = VerticalAlignment.Center
        };

        private readonly TabItem _database = new( ) {
            Name = "Database",
            Header = "Database",
            VerticalContentAlignment = VerticalAlignment.Center
        };

        private readonly TabItem _logging = new( ) {
            Name = "Logging",
            Header = "Logging",
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        private readonly TabItem _defaultLogging = new( ) {
            Name = "DefaultLog",
            Header = "Default Log",
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        private readonly TabItem _telemetryLogging = new( ) {
            Name = "TelemetryLog",
            Header = "Telemetry Log",
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        private readonly TabItem _consoleLogging = new( ) {
            Name = "ConsoleLog",
            Header = "Console Log",
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        private readonly TabItem _compression = new( ) {
            Name = "Compression",
            Header = "Compression",
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        private readonly TabItem _backBlaze = new( ) {
            Name = "BackBlaze",
            Header = "BackBlaze",
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        #endregion Fields


        #region Methods

        #region UpdateConfigSection

        internal void UpdateConfigSection( ICloudShareSyncConfig configSection ) {
            CompleteConfig config = CreateUpdatedConfig( configSection );
            _ = ConfigManager.ValidateAndAssignDefaults( config, null );
            CfgMgr.UpdateConfigSection( configSection );
        }

        private CompleteConfig CreateUpdatedConfig( ICloudShareSyncConfig configSection ) {
            CompleteConfig config = CfgMgr.Config;
            SetSyncConfig( configSection, config );
            SetLogConfig( configSection, config );
            SetBackBlazeConfig( configSection, config );
            SetCompressionConfig( configSection, config );
            SetDatabaseConfig( configSection, config );
            return config;
        }

        private static void SetSyncConfig( ICloudShareSyncConfig section, CompleteConfig complete ) {
            if (section is SyncConfig config) { complete.Sync = config; }
        }

        private static void SetLogConfig( ICloudShareSyncConfig section, CompleteConfig complete ) {
            if (section is Log4NetConfig config) { complete.Logging = config; }
        }

        private static void SetBackBlazeConfig( ICloudShareSyncConfig section, CompleteConfig complete ) {
            if (section is B2Config config) { complete.BackBlaze = config; }
        }

        private static void SetCompressionConfig( ICloudShareSyncConfig section, CompleteConfig complete ) {
            if (section is CompressionConfig config) { complete.Compression = config; }
        }

        private static void SetDatabaseConfig( ICloudShareSyncConfig section, CompleteConfig complete ) {
            if (section is DatabaseConfig config) { complete.Database = config; }
        }

        #endregion UpdateConfigSection


        #region SetTabContent

        internal void SetTabContent( IConfigurationTab? tab = null ) {
            CfgMgr = new ConfigManager( true );
            ReplaceOptionalDefaults( CfgMgr.Config );

            SetMainTabContent( tab );
            SetSyncTabContent( tab );
            SetDatabaseTabContent( tab );
            SetDefaultLogTabContent( tab );
            SeteTelemetryLogTabContent( tab );
            SetConsoleLogTabContent( tab );
            SetCompressionTabContent( tab );
            SetBackBlazeTabContent( tab );
        }

        private void SetMainTabContent( IConfigurationTab? tab ) {
            if ((tab is ConfigureMainTab) == false) {
                _main.Content = new ConfigureMainTab( CfgMgr.Config, CfgMgr._configPath, this );
            }
        }

        private void SetSyncTabContent( IConfigurationTab? tab ) {
            if ((tab is ConfigureSyncTab) == false) {
                _sync.Content = new ConfigureSyncTab( CfgMgr.Config.Sync, this );
            }
        }

        private void SetDatabaseTabContent( IConfigurationTab? tab ) {
            if ((tab is ConfigureDatabaseTab) == false) {
                _database.Content = new ConfigureDatabaseTab( CfgMgr.Config.Database, this );
            }
        }

        private void SetDefaultLogTabContent( IConfigurationTab? tab ) {
            if ((tab is ConfigureDefaultLogTab) == false) {
                _defaultLogging.Content = new ConfigureDefaultLogTab( CfgMgr.Config.Logging!, this );
            }
        }

        private void SeteTelemetryLogTabContent( IConfigurationTab? tab ) {
            if ((tab is ConfigureTelemetryLogTab) == false) {
                _telemetryLogging.Content = new ConfigureTelemetryLogTab( CfgMgr.Config.Logging!, this );
            }
        }

        private void SetConsoleLogTabContent( IConfigurationTab? tab ) {
            if ((tab is ConfigureConsoleLogTab) == false) {
                _consoleLogging.Content = new ConfigureConsoleLogTab( CfgMgr.Config.Logging!, this );
            }
        }

        private void SetCompressionTabContent( IConfigurationTab? tab ) {
            if ((tab is ConfigureCompressionTab) == false) {
                _compression.Content = new ConfigureCompressionTab( CfgMgr.Config.Compression!, this );
            }
        }

        private void SetBackBlazeTabContent( IConfigurationTab? tab ) {
            if ((tab is ConfigureBackBlazeTab) == false) {
                _backBlaze.Content = new ConfigureBackBlazeTab( CfgMgr.Config.BackBlaze!, this );
            }
        }

        #endregion SetTabContent

        private static void ReplaceOptionalDefaults( CompleteConfig config ) {
            config.Logging ??= new Log4NetConfig( );
            config.Logging.DefaultLogConfiguration ??= new DefaultLogConfig( );
            config.Logging.TelemetryLogConfiguration ??= new TelemetryLogConfig( );
            config.Logging.ConsoleConfiguration ??= new ConsoleLogConfig( );
            config.Compression ??= new CompressionConfig( );
            config.BackBlaze ??= new B2Config( );
        }

        #endregion Methods
    }
}
