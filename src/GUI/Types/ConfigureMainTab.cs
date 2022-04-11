using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureMainTab : StackPanel, IConfigurationTab {

        public ConfigureMainTab( CompleteConfig config, string configPath, ConfigureWindow parent ) {
            _parentWindow = parent;
            ConfigureWindowSettings( );
            ConfigureWindowContent( config, configPath );
        }


        #region Fields

        private readonly ConfigureWindow _parentWindow;

        #region ConfigPath

        private readonly TextBlock _configPathHeader = new( ) {
            Text = "Config Path: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly TextBlock _configPathTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            FontSize = 16
        };

        private readonly Button _updateConfigPathButton = new( ) {
            Name = "UpdateConfigPath",
            Margin = Thickness.Parse( "5,5,5,5" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            IsEnabled = true,
            Content = "Change Config Path"
        };

        private readonly TextBlock _noConfigTxt = new( ) {
            Name = "NoConfigTxt",
            Text = "Configuration file doesn't exist!",
            Foreground = Brushes.White,
            Background = Brushes.Red,
            VerticalAlignment = VerticalAlignment.Center
        };

        #endregion ConfigPath


        #region CompleteConfig

        private readonly Button _showJsonConfigButton = new( ) {
            Name = "ShowJsonConfig",
            Margin = Thickness.Parse( "5,5,5,5" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Content = "Show Settings Json",
            IsVisible = true,
        };

        private readonly Button _hideJsonConfigButton = new( ) {
            Name = "HideJsonConfig",
            Margin = Thickness.Parse( "5,5,5,5" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Content = "Hide Settings Json",
            IsVisible = false,
        };

        private readonly Button _saveJsonConfigButton = new( ) {
            Name = "SaveJsonConfig",
            Margin = Thickness.Parse( "5,5,5,5" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Content = "Save Json",
            IsVisible = false
        };

        private readonly TextBlock _completeConfigJsonHeader = new( ) {
            Text = "Json Config:",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            FontWeight = FontWeight.Bold,
            FontSize = 16,
            IsVisible = false
        };

        private readonly TextBox _completeConfigTxtBox = new( ) {
            Name = "ConfigTxtBox",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.WrapWithOverflow,
            IsUndoEnabled = true,
            AcceptsTab = true,
            TextAlignment = TextAlignment.Left,
            IsVisible = false
        };

        #endregion CompleteConfig

        #endregion Fields


        #region Methods

        private void ConfigureWindowSettings( ) {
            Name = "ConfigureMainPanel";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;
        }

        private void ConfigureWindowContent( CompleteConfig config, string configPath ) {
            NewConfigurationPathTextAndButton( configPath );
            NewJsonConfigBoxAndButton( config );
        }

        #region ConfigurationPath Methods

        private void NewConfigurationPathTextAndButton( string configPath ) {
            AddWrapPanelOne( configPath );
            AddWrapPanelTwo( configPath );

        }

        private void AddWrapPanelOne( string configPath ) {
            WrapPanel wrap = new( ) { Margin = Thickness.Parse( "0,5,5,5" ) };
            wrap.Children.Add( _configPathHeader );
            wrap.Children.Add( _configPathTxt );

            _configPathTxt.Text = configPath;

            Children.Add( wrap );
        }

        private void AddWrapPanelTwo( string configPath ) {
            WrapPanel wrap2 = new( ) { Margin = Thickness.Parse( "0,5,5,5" ) };

            wrap2.Children.Add( _updateConfigPathButton );
            _updateConfigPathButton.Click += ChangeConfigPathHandler;

            SetNoConfigTxtVisibility( configPath );
            wrap2.Children.Add( _noConfigTxt );

            Children.Add( wrap2 );
        }

        private void SetNoConfigTxtVisibility( string configPath ) {
            if (File.Exists( configPath )) {
                _noConfigTxt.IsVisible = false;
                _configPathTxt.Foreground = Brushes.Black;
                _configPathTxt.Background = Brushes.White;
            } else {
                _configPathTxt.Foreground = Brushes.White;
                _configPathTxt.Background = Brushes.Red;
                _noConfigTxt.IsVisible = true;
            }
        }

        #endregion ConfigurationPath Methods


        #region CompleteConfig Json Methods

        private void NewJsonConfigBoxAndButton( CompleteConfig config ) {
            _completeConfigTxtBox.Text = config.ToString( );
            _saveJsonConfigButton.Click += SaveJsonHandler;
            Children.Add( _completeConfigJsonHeader );
            Children.Add( _completeConfigTxtBox );
            AddWrapPanelThree( );
            Children.Add( _showJsonConfigButton );
            _showJsonConfigButton.Click += ShowHideJsonHandler;
            _hideJsonConfigButton.Click += ShowHideJsonHandler;
            AntiLinkShowHideJsonButtonVisibility( );
        }

        private void AddWrapPanelThree( ) {
            WrapPanel wrap3 = new( ) { Margin = Thickness.Parse( "5,15,5,15" ) };
            wrap3.Children.Add( _saveJsonConfigButton );
            wrap3.Children.Add( _hideJsonConfigButton );

            Children.Add( wrap3 );
        }

        private void AntiLinkShowHideJsonButtonVisibility( ) {
            IObservable<bool> showJsonStatus = _showJsonConfigButton.GetObservable( Button.IsVisibleProperty );
            IObservable<bool> hideJsonStatus = _hideJsonConfigButton.GetObservable( Button.IsVisibleProperty );

            _ = showJsonStatus.Subscribe( value => _hideJsonConfigButton.IsVisible = value == false );
            _ = hideJsonStatus.Subscribe( value => _showJsonConfigButton.IsVisible = value == false );


            _ = hideJsonStatus.Subscribe( value => _completeConfigJsonHeader.IsVisible = value );
            _ = hideJsonStatus.Subscribe( value => _completeConfigTxtBox.IsVisible = value );
            _ = hideJsonStatus.Subscribe( value => _saveJsonConfigButton.IsVisible = value );
        }

        #endregion CompleteConfig Json Methods


        #region SaveButton Methods

        private void WriteCompleteConfig( string config ) {
            File.WriteAllText( _configPathTxt.Text, config );
        }

        private CompleteConfig ReadCompleteConfig( ) =>
            CompleteConfig.FromString( File.ReadAllText( _configPathTxt.Text ) );

        #endregion SaveButton Methods


        #region Click Methods

        public async void ChangeConfigPathHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            try {
                button.IsEnabled = false;
                OpenFileDialog dialog = new( ) {
                    Title = "Select settings file",
                    InitialFileName = "AppSettings",
                    Filters = new( ) { new FileDialogFilter( ) { Name = "AppSettings", Extensions = { "json" } } },
                    AllowMultiple = false,
                };
                string[]? result = await dialog.ShowAsync( _parentWindow );
                if (result?.Length > 0) {
                    ConfigManager.SetAltDefaultConfigPath( result[0] );
                    _configPathTxt.Text = result[0];
                    SetNoConfigTxtVisibility( result[0] );
                    _completeConfigTxtBox.Text = ReadCompleteConfig( ).ToString( );
                    _parentWindow.SetTabContent( this );
                }
            } catch (Exception ex) {
                await new MessageBox(
                    "Failed to change config path.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            } finally {
                button.IsEnabled = true;
            }
        }

        public async void SaveJsonHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            try {
                button.IsEnabled = false;
                CompleteConfig config = CompleteConfig.FromString( _completeConfigTxtBox.Text );
                string validatedConfig = ConfigManager.ValidateAndAssignDefaults( config, null );
                WriteCompleteConfig( validatedConfig );
                CompleteConfig updatedConfig = ReadCompleteConfig( );
                _completeConfigTxtBox.Text = updatedConfig.ToString( );

                SetNoConfigTxtVisibility( _configPathTxt.Text );
                _parentWindow.SetTabContent( this );
                e.Handled = true;
                await Task.Delay( 250 );
            } catch (Exception ex) {
                await new MessageBox(
                "Failed to save json config.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            } finally {
                button.IsEnabled = true;
            }
        }

        public void ShowHideJsonHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            button.IsVisible = false;
            e.Handled = true;
        }

        #endregion Click Methods

        #endregion Methods

    }

}
