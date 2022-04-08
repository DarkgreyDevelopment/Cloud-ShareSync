using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureMainTab : StackPanel {

        private readonly Window _parentWindow;

        public readonly TextBlock ConfigPathHeader = new( ) {
            Text = "Config Path: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        public readonly TextBlock ConfigPathTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            FontSize = 16
        };

        public TextBox CompleteConfigTxtBox { get; private set; } = new TextBox( ) {
            Name = "ConfigTxtBox",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            IsUndoEnabled = true,
            AcceptsTab = true,
            TextAlignment = TextAlignment.Left
        };

        public readonly TextBlock CompleteConfigJsonHeader = new( ) {
            Text = "Json Config:",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        public readonly Button SaveJsonConfigButton = new( ) {
            Name = "SaveJsonConfig",
            Margin = Thickness.Parse( "5,5,5,5" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            IsEnabled = true,
            Content = "Save Json"
        };

        public readonly Button UpdateConfigPathButton = new( ) {
            Name = "UpdateConfigPath",
            Margin = Thickness.Parse( "5,5,5,5" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            IsEnabled = true,
            Content = "Change Config Path"
        };

        public readonly WrapPanel ConfigPathButtonWrapPanel = new( ) { Margin = Thickness.Parse( "0,5,5,5" ) };

        public TextBlock? _noConfigTxt = new( ) {
            Name = "NoConfigTxt",
            Text = "Configuration file doesn't exist!",
            Foreground = Brushes.White,
            Background = Brushes.Red,
            VerticalAlignment = VerticalAlignment.Center
        };

        public ConfigureMainTab( CompleteConfig config, string configPath, Window parent ) {
            _parentWindow = parent;
            ConfigureWindowSettings( );
            NewConfigurationPathTextAndButton( configPath );
            NewJsonConfigBoxAndButton( config );

            Children.Add( CompleteConfigJsonHeader );
            Children.Add( CompleteConfigTxtBox );
            Children.Add( SaveJsonConfigButton );
        }

        private void ConfigureWindowSettings( ) {
            Name = "ConfigureMainPanel";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;
        }

        private void NewConfigurationPathTextAndButton( string configPath ) {

            WrapPanel wrap = new( ) { Margin = Thickness.Parse( "0,5,5,5" ) };
            wrap.Children.Add( ConfigPathHeader );
            wrap.Children.Add( ConfigPathTxt );

            ConfigPathButtonWrapPanel.Children.Add( UpdateConfigPathButton );
            UpdateConfigPathButton.Click += ChangeConfigPathHandler;
            ConfigPathTxt.Text = configPath;
            if (File.Exists( configPath )) {
                _noConfigTxt = null;
            } else {
                ConfigPathTxt.Foreground = Brushes.White;
                ConfigPathTxt.Background = Brushes.Red;
                ConfigPathButtonWrapPanel.Children.Add(
                    _noConfigTxt
                );
            }

            Children.Add( wrap );
            Children.Add( ConfigPathButtonWrapPanel );

        }

        private void NewJsonConfigBoxAndButton( CompleteConfig config ) {
            CompleteConfigTxtBox.Text = config.ToString( );
            SaveJsonConfigButton.Click += SaveJsonHandler;
        }

        public async void ChangeConfigPathHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            try {
                button.IsEnabled = false;
                OpenFileDialog dialog = new( );
                dialog.Filters.Add( new FileDialogFilter( ) { Name = "AppSettings", Extensions = { "json" } } );
                string[]? result = await dialog.ShowAsync( _parentWindow );
                if (result?.Length > 0) {
                    ConfigManager.SetAltDefaultConfigPath( result[0] );
                    await new MessageBox(
                        "Window Reload Required.",
                        "Please re-open the configuration window to make additional changes.",
                        null
                    ).ShowDialog( );
                    _parentWindow.Close( );
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
                CompleteConfig config = CompleteConfig.FromString( CompleteConfigTxtBox.Text );
                string validatedConfig = ConfigManager.ValidateAndAssignDefaults( config, null );
                WriteCompleteConfig( validatedConfig );
                CompleteConfigTxtBox.Text = ReadCompleteConfig( ).ToString( );
                if (_noConfigTxt != null) {
                    _ = ConfigPathButtonWrapPanel.Children.Remove( _noConfigTxt );
                    ConfigPathTxt.Foreground = Brushes.Black;
                    ConfigPathTxt.Background = Brushes.White;
                }
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

        private void WriteCompleteConfig( string config ) {
            File.WriteAllText( ConfigPathTxt.Text, config );
        }

        private CompleteConfig ReadCompleteConfig( ) =>
            CompleteConfig.FromString( File.ReadAllText( ConfigPathTxt.Text ) );
    }

}
