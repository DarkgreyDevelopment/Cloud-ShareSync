﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI {
    public partial class App : Application {

        public override void Initialize( ) { AvaloniaXamlLoader.Load( this ); }

        public override void OnFrameworkInitializationCompleted( ) {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow( );
            }
            base.OnFrameworkInitializationCompleted( );
        }
    }
}
