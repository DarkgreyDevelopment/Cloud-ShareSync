using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cloud_ShareSync.Backup;
using Cloud_ShareSync.GUI.ViewModels;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI {
    public partial class App : Application {

        public override void Initialize( ) {
            AvaloniaXamlLoader.Load( this );
        }

        public override void OnFrameworkInitializationCompleted( ) {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                MainWindow mainWindow = new( ) {
                    DataContext = new MainWindowViewModel( )
                };

                mainWindow.Title = "Cloud-ShareSync";
                StackPanel mainPanel = mainWindow.FindControl<StackPanel>( "MainPanel" );

                ConfigureSettingsButton( mainPanel );
                ConfigureBackupButton( mainPanel );
                ConfigureRestoreButton( mainPanel );
                ConfigureFunButton( mainPanel );

                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted( );
        }

        internal static void ConfigureFunButton( StackPanel mainPanel ) {
            Button funButton = mainPanel.FindControl<Button>( "FunButton" );

            funButton.Content = "Don't Press.";
            funButton.Click += ( _, _ ) => {
                try {
                    funButton.IsEnabled = false;
                    funButton.IsVisible = false;
                } catch (Exception ex) {
                    Console.WriteLine( "ConfigureFunButton Catch" );
                    ErrorDialog
                        .Show(
                            "Fun Process Failed.",
                            ex.Message,
                            ex.StackTrace
                        ).GetAwaiter( )
                        .GetResult( );
                }
            };
        }

        internal static void ConfigureRestoreButton( StackPanel mainPanel ) {
            Button restoreButton = mainPanel.FindControl<Button>( "RestoreButton" );
            restoreButton.Content = "Restore";
            restoreButton.IsEnabled = false;
            restoreButton.Click += ( _, _ ) => {
                try {
                } catch (Exception ex) {
                    Console.WriteLine( "ConfigureRestoreButton Catch" );
                    ErrorDialog
                        .Show(
                            "Restore Process Failed.",
                            ex.Message,
                            ex.StackTrace
                        ).GetAwaiter( )
                        .GetResult( );
                }
            };
        }

        internal static void ConfigureBackupButton( StackPanel mainPanel ) {
            Button backupButton = mainPanel.FindControl<Button>( "BackupButton" );
            backupButton.Content = "Backup";
            backupButton.Click += async ( _, _ ) => {
                if ((backupButton.Content as string) == "Backup") {
                    try {
                        backupButton.Content = "Backup In Progress";
                        backupButton.IsEnabled = false;
                        await Task.Run( ( ) => {
                            Process backup = new( );
                            _ = backup.Run( );
                        } );
                        backupButton.Content = "Backup Completed";
                    } catch (Exception ex) {
                        Console.WriteLine( "ConfigureBackupButton Catch" );
                        ErrorDialog
                            .Show(
                                "Backup Process Failed.",
                                ex.Message,
                                ex.StackTrace
                            ).GetAwaiter( )
                            .GetResult( );
                    } finally {
                        backupButton.IsEnabled = true;
                    }
                }
            };
        }

        internal static void ConfigureSettingsButton( StackPanel mainPanel ) {
            Button settingsButton = mainPanel.FindControl<Button>( "SettingsButton" );
            settingsButton.Content = "Configure";
            settingsButton.Click += ( _, _ ) => {
                if ((settingsButton.Content as string) == "Configure") {
                    try {

                    } catch (Exception ex) {
                        Console.WriteLine( "ConfigureSettingsButton Catch" );
                        ErrorDialog
                            .Show(
                                "Configure Process Failed.",
                                ex.Message,
                                ex.StackTrace
                            ).GetAwaiter( )
                            .GetResult( );
                    }
                }
            };
        }
    }
}
