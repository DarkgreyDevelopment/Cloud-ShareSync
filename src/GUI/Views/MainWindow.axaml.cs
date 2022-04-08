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
            Name = "BannerLogo",
            Margin = Thickness.Parse( "10,5,10,5" ),
            IsEnabled = true,
            IsVisible = true,
            Source = new Bitmap(
                AssetLoader?.Open(
                    new Uri( "resm:Cloud_ShareSync.GUI.Assets.BannerLogo.png" )
                )
            )
        };

        public Image Status3 { get; } = new Image( ) {
            Name = "StatusInidicator3",
            Margin = Thickness.Parse( "10,5,10,5" ),
            IsEnabled = false,
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Source = new Bitmap(
                AssetLoader?.Open(
                    new Uri( "resm:Cloud_ShareSync.GUI.Assets.statusInidicator3.png" )
                )
            )
        };

        public Image Status2 { get; } = new Image( ) {
            Name = "StatusInidicator2",
            Margin = Thickness.Parse( "10,5,10,5" ),
            IsEnabled = false,
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Source = new Bitmap(
                AssetLoader?.Open(
                    new Uri( "resm:Cloud_ShareSync.GUI.Assets.statusInidicator2.png" )
                )
            )
        };

        public Image Status1 { get; } = new Image( ) {
            Name = "StatusInidicator1",
            Margin = Thickness.Parse( "10,5,10,5" ),
            IsEnabled = false,
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Source = new Bitmap(
                AssetLoader?.Open(
                    new Uri( "resm:Cloud_ShareSync.GUI.Assets.statusInidicator1.png" )
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

        public Button FunButton { get; } = new( ) {
            Name = "FunButton",
            Margin = Thickness.Parse( "10,5,10,5" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            IsEnabled = true,
            Content = "Don't Touch."
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

            MainPanel.Children.Add( FunButton );
            MainPanel.Children.Add( Status1 );
            MainPanel.Children.Add( Status2 );
            MainPanel.Children.Add( Status3 );
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

            Grid.SetColumn( Status3, 0 );
            Grid.SetColumn( Status2, 0 );
            Grid.SetColumn( Status1, 0 );
            Grid.SetColumn( FunButton, 0 );

        }

        private void SetGridRows( ) {
            Grid.SetRow( MainPanel, 0 );
            Grid.SetRow( BannerLogo, 0 );
            Grid.SetRow( ConfigureButton, 1 );
            Grid.SetRow( BackupButton, 2 );
            Grid.SetRow( RestoreButton, 3 );
            Grid.SetRow( SyncButton, 4 );

            Grid.SetRow( Status3, 5 );
            Grid.SetRow( Status2, 5 );
            Grid.SetRow( Status1, 5 );
            Grid.SetRow( FunButton, 5 );
        }

        #endregion Configure Grid

        #endregion Configure MainPanel

        private async Task AnimateLoadingIndicator( ) {
            int count = 0;
            do {
                ChangeControlVisualizationStatus( Status3, false );
                if (count % 3 == 0) {
                    ChangeControlVisualizationStatus( Status1, true );
                    await Task.Delay( 750 );
                    ChangeControlVisualizationStatus( Status1, false );
                } else {
                    ChangeControlVisualizationStatus( Status2, true );
                    await Task.Delay( 500 );
                }
                ChangeControlVisualizationStatus( Status2, false );
                ChangeControlVisualizationStatus( Status3, true );
                await Task.Delay( 500 );
                count++;
            } while (count <= 9);
        }

        private void ResetButtonState( Button button ) {
            ChangeControlVisualizationStatus( Status1, false );
            ChangeControlVisualizationStatus( Status2, false );
            ChangeControlVisualizationStatus( Status3, false );
            ChangeControlVisualizationStatus( button, true );
        }

        private static void ChangeControlVisualizationStatus( Control control, bool status ) {
            control.IsEnabled = status;
            control.IsVisible = status;
        }


        #region Button Clicks

        private void ConfigureButtonClicks( ) {
            ConfigureButton.Click += ClickConfigureButton;
            BackupButton.Click += ClickBackupButton;
            RestoreButton.Click += ClickRestoreButton;
            SyncButton.Click += ClickSyncButton;
            FunButton.Click += ClickFunButton;
        }

        private async void ClickConfigureButton( object? sender, RoutedEventArgs e ) {
            try {
                await new ConfigureWindow( ).ShowDialog( this );
            } catch (Exception ex) {
                await new MessageBox(
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
                } catch (Exception ex) {
                    await new MessageBox(
                        "Backup Process Failed.",
                        ex.Message,
                        ex.StackTrace
                    ).ShowDialog( );
                } finally {
                    BackupButton.Content = "Backup";
                    BackupButton.IsEnabled = true;
                    BannerLogo.IsVisible = true;
                }
            }
        }

        private async void ClickRestoreButton( object? sender, RoutedEventArgs e ) {
            try {
            } catch (Exception ex) {
                await new MessageBox(
                    "Restore Process Failed.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            }
        }

        private async void ClickSyncButton( object? sender, RoutedEventArgs e ) {
            try {
            } catch (Exception ex) {
                await new MessageBox(
                    "Sync Process Failed.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            }
        }

        private async void ClickFunButton( object? sender, RoutedEventArgs e ) {
            try {
                ChangeControlVisualizationStatus( FunButton, false );
                await AnimateLoadingIndicator( );
                ResetButtonState( FunButton );
                FunButton.Content = "I said dont touch me!";
                FunButton.IsEnabled = false;
            } catch (Exception ex) {
                await new MessageBox(
                    "Fun Process Failed.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            }
        }


        #endregion Button Clicks

        private static async Task RunBackupProcess( ) {
            await Task.Run( ( ) => {
                Process backup = new( );
                _ = backup.Run( );
            } );
        }

    }
}
