using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI {
    public partial class App : Application {

        internal static readonly IAssetLoader? AssetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>( );
        internal static readonly WindowIcon Icon = new(
            AssetLoader?.Open( new Uri( @"resm:Cloud_ShareSync.GUI.Assets.logo.ico" ) )
        );

        public override void Initialize( ) {
            Avalonia.Themes.Fluent.FluentTheme theme = new( new Uri( "http://schemas.microsoft.com/winfx/2006/xaml" ) );
            theme.Mode = Avalonia.Themes.Fluent.FluentThemeMode.Light;
            Styles.Add( theme );
        }

        public override void OnFrameworkInitializationCompleted( ) {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow( );
            }
            base.OnFrameworkInitializationCompleted( );
        }
    }
}
