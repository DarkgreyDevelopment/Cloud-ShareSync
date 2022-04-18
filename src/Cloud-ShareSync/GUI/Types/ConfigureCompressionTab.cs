using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Configuration.Types;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureCompressionTab : StackPanel, IConfigurationTab {

        public ConfigureCompressionTab( CompressionConfig config, ConfigureWindow parent ) {
            _parentWindow = parent;
            ConfigureWindowSettings( );
            ConfigureWindowContent( config );
        }


        #region Fields

        private readonly ConfigureWindow _parentWindow;

        private readonly Button _saveButton = new( ) {
            Name = "SaveCompressionConfig",
            Margin = Thickness.Parse( "5,15,5,15" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Save"
        };

        private readonly TextBlock _dependencyPathHeader = new( ) {
            Text = "Dependency Path: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly TextBlock _dependencyPathTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly Button _updateDependencyPathButton = new( ) {
            Name = "UpdateDependencyPath",
            Margin = Thickness.Parse( "5,5,5,15" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            IsEnabled = true,
            Content = "Change Dependency Path"
        };

        #endregion Fields


        #region Methods

        private void ConfigureWindowSettings( ) {
            Name = "ConfigureCompressionTab";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;
        }

        private void ConfigureWindowContent( CompressionConfig config ) {
            _dependencyPathTxt.Text = config.DependencyPath;
            _updateDependencyPathButton.Click += ChangeDependencyPathHandler;
            Children.Add( _dependencyPathHeader );
            Children.Add( _dependencyPathTxt );
            Children.Add( _updateDependencyPathButton );
            AddSaveButton( );
        }

        #region SaveButton Methods

        private void AddSaveButton( ) {
            _saveButton.Click += SaveConfigHandler;
            Children.Add( _saveButton );
        }

        private CompressionConfig GetCompressionConfig( ) => new( ) {
            DependencyPath = _dependencyPathTxt.Text
        };

        #endregion SaveButton Methods


        #region Click Methods

        public async void ChangeDependencyPathHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            try {
                button.IsEnabled = false;
                OpenFileDialog dialog = new( ) {
                    Title = "Select 7z dependency.",
                    InitialFileName = "7z",
                    Filters = new( ) { new FileDialogFilter( ) { Name = "7z", Extensions = { "dll", "exe", "" } } },
                    AllowMultiple = false,
                };
                string[]? result = await dialog.ShowAsync( _parentWindow );
                if (result?.Length > 0) { _dependencyPathTxt.Text = result[0]; }
            } catch (Exception ex) {
                await new MessageBox(
                    "Failed to change sqlite db folder path.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            } finally {
                button.IsEnabled = true;
            }
        }

        public async void SaveConfigHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            button.IsEnabled = false;
            try {
                _parentWindow.UpdateConfigSection( GetCompressionConfig( ) );
                _parentWindow.SetTabContent( this );
                await Task.Delay( 250 );
            } catch (Exception ex) {
                await new MessageBox(
                    "Unable to save compression config.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            } finally {
                button.IsEnabled = true;
            }
        }

        #endregion Click Methods

        #endregion Methods

    }

}
