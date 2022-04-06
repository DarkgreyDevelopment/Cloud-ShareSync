using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cloud_ShareSync.Backup;
using ReactiveUI;

namespace Cloud_ShareSync.GUI.Views {
    public partial class MainWindow : Window {

        #region Fields

        internal static readonly IAssetLoader? AssetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>( );

        public StackPanel MainPanel { get; } = new( ) {
            Name = "MainPanel",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Orientation = Orientation.Vertical,
        };

        public Image BannerLogo { get; } = new Image( ) {
            Margin = Thickness.Parse( "10,5,10,5" ),
            IsEnabled = true,
            IsVisible = true,
            Name = "BannerLogo",
            Source = new Bitmap(
                AssetLoader?.Open(
                    new Uri( "resm:Cloud_ShareSync.GUI.Assets.BannerLogo.png" )
                )
            )
        };

        public Button ConfigureButton { get; } = new( ) {
            Name = "ConfigureButton",
            Margin = Thickness.Parse( "10,5,10,2" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Configure"
        };

        public Button BackupButton { get; } = new( ) {
            Name = "BackupButton",
            Margin = Thickness.Parse( "10,3,10,2" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Backup"
        };


        public Button RestoreButton { get; } = new( ) {
            Name = "RestoreButton",
            Margin = Thickness.Parse( "10,3,10,2" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Restore"
        };

        public Button SyncButton { get; } = new( ) {
            Name = "SyncButton",
            Margin = Thickness.Parse( "10,3,10,5" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = false,
            Content = "Sync"
        };

        #endregion Fields


        public MainWindow( ) {
            //InitializeComponent( false );
            ConfigureWindowProperties( );
            ConfigureButtonClicks( );
            ConfigureMainPanel( );
            Content = MainPanel;
        }

        private void ConfigureWindowProperties( ) {
            Title = "Cloud-ShareSync";
            DataContext = new ReactiveObject( );
            Icon = new WindowIcon(
                AssetLoader?.Open(
                        new Uri( @"resm:Cloud_ShareSync.GUI.Assets.logo.ico" )
                )
            );
            Height = 400;
            Width = 400;
            SizeToContent = SizeToContent.Manual;
        }

        #region Configure MainPanel

        private void ConfigureMainPanel( ) {
            ConfigureGrid( );
            AddMainPanelChildren( );
        }

        private void AddMainPanelChildren( ) {
            MainPanel.Children.Add( BannerLogo );
            MainPanel.Children.Add( ConfigureButton );
            MainPanel.Children.Add( BackupButton );
            MainPanel.Children.Add( RestoreButton );
            MainPanel.Children.Add( SyncButton );
        }

        #region Configure Grid

        private void ConfigureGrid( ) {
            SetGridColumn( );
            SetGridRows( );
        }

        private void SetGridColumn( ) {
            Grid.SetColumn( MainPanel, 0 );
            Grid.SetColumn( BannerLogo, 0 );
            Grid.SetColumn( ConfigureButton, 0 );
            Grid.SetColumn( BackupButton, 0 );
            Grid.SetColumn( RestoreButton, 0 );
            Grid.SetColumn( SyncButton, 0 );
        }

        private void SetGridRows( ) {
            Grid.SetRow( MainPanel, 0 );
            Grid.SetRow( BannerLogo, 0 );
            Grid.SetRow( ConfigureButton, 1 );
            Grid.SetRow( BackupButton, 2 );
            Grid.SetRow( RestoreButton, 3 );
            Grid.SetRow( SyncButton, 4 );
        }

        #endregion Configure Grid

        #endregion Configure MainPanel


        #region Button Clicks

        private void ConfigureButtonClicks( ) {
            ConfigureButton.Click += ClickConfigureButton;
            BackupButton.Click += ClickBackupButton;
            RestoreButton.Click += ClickRestoreButton;
            SyncButton.Click += ClickSyncButton;
        }

        private async void ClickConfigureButton( object? sender, RoutedEventArgs e ) {
            try {
            } catch (Exception ex) {
                await new ErrorDialog(
                    "Configure Process Failed.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            }
        }

        private async void ClickBackupButton( object? sender, RoutedEventArgs e ) {
            if ((BackupButton.Content as string) == "Backup") {
                try {
                    BackupButton.Content = "Backup In Progress";
                    BackupButton.IsEnabled = false;
                    BannerLogo.IsVisible = false;
                    await RunBackupProcess( );
                    BackupButton.Content = "Backup Completed";
                    BackupButton.IsEnabled = true;
                } catch (Exception ex) {
                    await new ErrorDialog(
                        "Backup Process Failed.",
                        ex.Message,
                        ex.StackTrace
                    ).ShowDialog( );
                } finally {
                    BackupButton.Content = "Backup";
                    BannerLogo.IsVisible = true;
                }
            }
        }

        private async void ClickRestoreButton( object? sender, RoutedEventArgs e ) {
            try {
            } catch (Exception ex) {
                await new ErrorDialog(
                    "Restore Process Failed.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            }
        }

        private async void ClickSyncButton( object? sender, RoutedEventArgs e ) {
            try {
            } catch (Exception ex) {
                await new ErrorDialog(
                    "Sync Process Failed.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            }
        }

        #endregion Button Clicks

        private async Task RunBackupProcess( ) {
            await Task.Run( ( ) => {
                Process backup = new( );
                _ = backup.Run( );
            } );
        }

    }
}
