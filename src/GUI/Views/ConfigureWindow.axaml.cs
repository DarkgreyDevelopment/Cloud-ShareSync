using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.GUI.Types;

namespace Cloud_ShareSync.GUI.Views {
    public partial class ConfigureWindow : Window {
        public ConfigManager CfgMgr { get; private set; }

        public TabItem Main { get; private set; }
        public TabItem Sync { get; private set; }
        public TabItem Database { get; private set; }
        public TabItem Logging { get; private set; }
        public TabItem DefaultLogging { get; private set; }
        public TabItem TelemetryLogging { get; private set; }
        public TabItem ConsoleLogging { get; private set; }
        public TabItem Compression { get; private set; }
        public TabItem BackBlaze { get; private set; }

        public ConfigureWindow( ) {
            AvaloniaXamlLoader.Load( this );
            CfgMgr = new ConfigManager( true );
            CompleteConfig config = CfgMgr.Config;

            ReplaceOptionalDefaults( config );

            Main = this.FindControl<TabItem>( "Main" );
            Main.Content = new ConfigureMainTab( config, CfgMgr._configPath, this );

            Sync = this.FindControl<TabItem>( "Sync" );

            Database = this.FindControl<TabItem>( "Database" );

            Logging = this.FindControl<TabItem>( "Logging" );

            DefaultLogging = this.FindControl<TabItem>( "DefaultLog" );

            TelemetryLogging = this.FindControl<TabItem>( "TelemetryLog" );

            ConsoleLogging = this.FindControl<TabItem>( "ConsoleLog" );

            Compression = this.FindControl<TabItem>( "Compression" );

            BackBlaze = this.FindControl<TabItem>( "BackBlaze" );

            Height = 1200;
            Width = 1200;
        }

        public static void ReplaceOptionalDefaults( CompleteConfig config ) {
            config.Logging ??= new Log4NetConfig( );
            config.Logging.DefaultLogConfiguration ??= new DefaultLogConfig( );
            config.Logging.TelemetryLogConfiguration ??= new TelemetryLogConfig( );
            config.Logging.ConsoleConfiguration ??= new ConsoleLogConfig( );
            config.Compression ??= new CompressionConfig( );
            config.BackBlaze ??= new B2Config( );
        }
    }
}
