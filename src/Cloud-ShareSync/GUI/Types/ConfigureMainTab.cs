using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Configuration.ManagedActions;
using Cloud_ShareSync.Configuration.Types;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureMainTab : StackPanel, IConfigurationTab {

        public ConfigureMainTab( CompleteConfig config, string configPath, ConfigureWindow parent ) {
            _parentWindow = parent;
            ConfigureWindowSettings( );
            _configPath = new(
                "Config Path: ",
                configPath,
                "Change Config Path",
                ChangeConfigPathHandler,
                "Configuration file doesn't exist!"
            );
            _configPath.WarnOnMissingPath( configPath, true );
            ConfigureWindowContent( config );
        }


        #region Fields

        private readonly ConfigureWindow _parentWindow;

        private readonly PathStack _configPath;


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

        private void ConfigureWindowContent( CompleteConfig config ) {
            Children.Add( _configPath );
            NewJsonConfigBoxAndButton( config );
        }

        #region ConfigurationPath Methods

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
            File.WriteAllText( _configPath.PathText, config );
        }

        private CompleteConfig ReadCompleteConfig( ) =>
            CompleteConfig.FromString( File.ReadAllText( _configPath.PathText ) );

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
                    ConfigPathHandler.SetAltDefaultConfigPath( result[0] );
                    _configPath.PathText = result[0];
                    _configPath.WarnOnMissingPath( result[0], true );
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
                string validatedConfig = CompleteConfigBuilder.ValidateAndAssignDefaults( config, false );
                WriteCompleteConfig( validatedConfig );
                CompleteConfig updatedConfig = ReadCompleteConfig( );
                _completeConfigTxtBox.Text = updatedConfig.ToString( );
                _configPath.WarnOnMissingPath( _configPath.PathText, true );
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
