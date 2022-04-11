using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Cloud_ShareSync.Backup;
using ReactiveUI;

namespace Cloud_ShareSync.GUI.Views {
    public partial class MainWindow : Window {

        public MainWindow( ) {
            ConfigureWindowProperties( );
            ConfigureButtonClicks( );
            AddMainPanelContent( );
        }

        #region Fields

        private CancellationTokenSource _cancelSource = new( );

        private readonly StackPanel _panel = new( ) {
            Name = "MainPanel",
            Margin = Thickness.Parse( "5,5,5,5" ),
            IsEnabled = true,
            IsVisible = true,
        };

        private readonly Image _bannerLogo = new( ) {
            Name = "BannerLogo",
            Margin = Thickness.Parse( "10,5,10,5" ),
            IsEnabled = true,
            IsVisible = true,
            Source = new Bitmap(
                App.AssetLoader?.Open(
                    new Uri( "resm:Cloud_ShareSync.GUI.Assets.BannerLogo.png" )
                )
            )
        };

        private readonly Image _status3 = new( ) {
            Name = "StatusIndicator3",
            Margin = Thickness.Parse( "10,5,10,5" ),
            IsEnabled = false,
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Source = new Bitmap(
                App.AssetLoader?.Open(
                    new Uri( "resm:Cloud_ShareSync.GUI.Assets.StatusIndicator3.png" )
                )
            )
        };

        private readonly Image _status2 = new( ) {
            Name = "StatusIndicator2",
            Margin = Thickness.Parse( "10,5,10,5" ),
            IsEnabled = false,
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Source = new Bitmap(
                App.AssetLoader?.Open(
                    new Uri( "resm:Cloud_ShareSync.GUI.Assets.StatusIndicator2.png" )
                )
            )
        };

        private readonly Image _status1 = new( ) {
            Name = "StatusIndicator1",
            Margin = Thickness.Parse( "10,5,10,5" ),
            IsEnabled = false,
            IsVisible = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Source = new Bitmap(
                App.AssetLoader?.Open(
                    new Uri( "resm:Cloud_ShareSync.GUI.Assets.StatusIndicator1.png" )
                )
            )
        };

        private readonly Button _configureButton = new( ) {
            Name = "ConfigureButton",
            Margin = Thickness.Parse( "10,5,10,2" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Configure"
        };

        private readonly Button _backupButton = new( ) {
            Name = "BackupButton",
            Margin = Thickness.Parse( "10,3,10,2" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Backup"
        };


        private readonly Button _restoreButton = new( ) {
            Name = "RestoreButton",
            Margin = Thickness.Parse( "10,3,10,2" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Restore"
        };

        private readonly Button _syncButton = new( ) {
            Name = "SyncButton",
            Margin = Thickness.Parse( "10,3,10,5" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = false,
            Content = "Sync"
        };

        #endregion Fields


        #region Methods

        private void ConfigureWindowProperties( ) {
            Title = "Cloud-ShareSync";
            DataContext = new ReactiveObject( );
            Icon = App.Icon;
            Height = 400;
            Width = 400;
            SizeToContent = SizeToContent.Manual;
        }

        #region Configure Panel Methods

        private void AddMainPanelContent( ) {
            _panel.Children.Add( _status1 );
            _panel.Children.Add( _status2 );
            _panel.Children.Add( _status3 );
            _panel.Children.Add( _bannerLogo );
            _panel.Children.Add( _configureButton );
            _panel.Children.Add( _backupButton );
            _panel.Children.Add( _restoreButton );
            _panel.Children.Add( _syncButton );
            Content = _panel;
        }

        #endregion Configure Panel Methods


        #region Banner Animation Methods

        private async Task AnimateLoadingIndicator( CancellationToken token ) {
            ChangeControlVisualizationStatus( _bannerLogo, false );

            int count = 0;
            do {
                ChangeControlVisualizationStatus( _status3, false );
                if (count % 5 == 0) {
                    ChangeControlVisualizationStatus( _status1, true );
                    await Task.Delay( 750, token );
                    ChangeControlVisualizationStatus( _status1, false );
                } else {
                    ChangeControlVisualizationStatus( _status2, true );
                    await Task.Delay( 500, token );
                }
                ChangeControlVisualizationStatus( _status2, false );
                ChangeControlVisualizationStatus( _status3, true );
                await Task.Delay( 500, token );
                if (count < 100) {
                    count++;
                } else {
                    count = 0;
                }
            } while (token.IsCancellationRequested == false);
        }

        private void ResetControlState( Control control ) {
            ChangeControlVisualizationStatus( _status1, false );
            ChangeControlVisualizationStatus( _status2, false );
            ChangeControlVisualizationStatus( _status3, false );
            ChangeControlVisualizationStatus( control, true );
        }

        private static void ChangeControlVisualizationStatus( Control control, bool status ) {
            control.IsEnabled = status;
            control.IsVisible = status;
        }

        #endregion Banner Animation Methods


        #region Button Handling Methods

        private void DisableButtons( ) {
            _backupButton.IsEnabled = false;
            _configureButton.IsEnabled = false;
            _restoreButton.IsEnabled = false;
            _bannerLogo.IsVisible = false;
        }

        private void EnableButtons( ) {
            _backupButton.IsEnabled = true;
            _configureButton.IsEnabled = true;
            _restoreButton.IsEnabled = true;
            _bannerLogo.IsVisible = true;
        }

        private void StartButtonPress( ) {
            DisableButtons( );
            CancellationToken token = _cancelSource.Token;
            _ = AnimateLoadingIndicator( token );
        }

        private void StopButtonPress( ) {
            _cancelSource.Cancel( );
            ResetControlState( _bannerLogo );
            EnableButtons( );
            _cancelSource = new( );
        }


        #region Button Clicks

        private void ConfigureButtonClicks( ) {
            _configureButton.Click += ClickConfigureButton;
            _backupButton.Click += ClickBackupButton;
            _restoreButton.Click += ClickRestoreButton;
            _syncButton.Click += ClickSyncButton;
        }

        private async void ClickConfigureButton( object? sender, RoutedEventArgs e ) {
            StartButtonPress( );
            try {
                await new ConfigureWindow( ).ShowDialog( this );
            } catch (Exception ex) {
                await new MessageBox(
                    "Configure Process Failed.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            }
            StopButtonPress( );
        }

        private async void ClickBackupButton( object? sender, RoutedEventArgs e ) {
            if ((_backupButton.Content as string) == "Backup") {
                StartButtonPress( );
                try {
                    _backupButton.Content = "Backup In Progress";
                    await RunBackupProcess( );
                } catch (Exception ex) {
                    await new MessageBox(
                        "Backup Process Failed.",
                        ex.Message,
                        ex.StackTrace
                    ).ShowDialog( );
                } finally {
                    _backupButton.Content = "Backup";
                }
                StopButtonPress( );
            }
        }

        private async void ClickRestoreButton( object? sender, RoutedEventArgs e ) {
            StartButtonPress( );
            try {
                _restoreButton.Content = "Restore In Progress";
            } catch (Exception ex) {
                await new MessageBox(
                    "Restore Process Failed.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            } finally {
                _restoreButton.Content = "Restore";
            }
            StopButtonPress( );
        }

        private async void ClickSyncButton( object? sender, RoutedEventArgs e ) {
            StartButtonPress( );
            try {
                _syncButton.Content = "Sync Progress Running";
            } catch (Exception ex) {
                await new MessageBox(
                    "Sync Process Failed.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            } finally {
                _syncButton.Content = "Sync";
            }
            StopButtonPress( );
        }

        #endregion Button Clicks

        #endregion Button Handling Methods

        private static async Task RunBackupProcess( ) {
            await Task.Run( ( ) => {
                Process backup = new( );
                _ = backup.Run( );
            } );
        }

        #endregion Methods
    }
}
