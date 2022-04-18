using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Configuration.Types;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureTelemetryLogTab : BaseTab {

        public ConfigureTelemetryLogTab( Log4NetConfig config, ConfigureWindow parent ) : base( config, parent ) {
            _config = config;
            ConfigureWindowSettings( );
            ConfigureWindowContent( config );
        }

        #region Fields

        private readonly Log4NetConfig _config;

        private readonly Button _saveButton = new( ) {
            Name = "SaveTelemetryLogConfig",
            Margin = Thickness.Parse( "5,15,5,15" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Save"
        };

        private readonly CheckBox _enableTelemetryLogCheckBox = new( ) {
            IsChecked = true,
            Content = "Enable Telemetry Log",
            Margin = Thickness.Parse( "5,5,5,5" )
        };


        #region FileName

        private readonly WrapPanel _fileNamePanel = new( ) {
            Margin = Thickness.Parse( "5,0,5,15" )
        };

        private readonly TextBlock _fileNameHeader = new( ) {
            Text = "FileName: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly TextBox _fileNameTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 16
        };

        #endregion FileName


        #region LogDirectory

        private readonly WrapPanel _logDirectoryPanel = new( ) {
            Margin = Thickness.Parse( "5,0,5,15" )
        };

        private readonly TextBlock _logDirectoryHeader = new( ) {
            Text = "Log Directory: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly TextBlock _logDirectoryTxt = new( ) {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 16
        };

        private readonly Button _updateLogDirectoryButton = new( ) {
            Name = "UpdateLogDirectory",
            Margin = Thickness.Parse( "5,5,5,15" ),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            IsEnabled = true,
            Content = "Update Log Directory"
        };

        #endregion LogDirectory


        #region RolloverCount

        private readonly WrapPanel _rolloverCountPanel = new( ) {
            Margin = Thickness.Parse( "5,0,5,15" )
        };

        private readonly TextBlock _rolloverCountHeader = new( ) {
            Text = "Rollover Count: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly NumericUpDown _rolloverCount = new( ) {
            Minimum = 0,
            Increment = 1
        };

        #endregion RolloverCount


        #region MaximumSize

        private readonly WrapPanel _maximumSizePanel = new( ) {
            Margin = Thickness.Parse( "5,0,5,15" )
        };

        private readonly TextBlock _maximumSizeHeader = new( ) {
            Text = "Maximum Size (MB): ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly NumericUpDown _maximumSize = new( ) {
            Minimum = 1,
            Increment = 1
        };

        #endregion MaximumSize

        #endregion Fields


        #region Methods

        private void ConfigureWindowContent( Log4NetConfig config ) {
            AddEnableTelemetryLogCheckBox( config.EnableTelemetryLog );
            AddFileName( config.TelemetryLogConfiguration?.FileName ?? "" );
            AddLogDirectory( config.TelemetryLogConfiguration?.LogDirectory ?? "" );
            AddRolloverCount( config.TelemetryLogConfiguration?.RolloverCount ?? 0 );
            AddMaximumSize( config.TelemetryLogConfiguration?.MaximumSize ?? 1 );
            AddSaveButton( );
            LinkEnableCheckboxAndRemainingVisiblity( );
        }

        #region EnableTelemetryLog Methods

        private void AddEnableTelemetryLogCheckBox( bool state ) {
            _enableTelemetryLogCheckBox.IsChecked = state;
            Children.Add( _enableTelemetryLogCheckBox );
        }

        private void LinkEnableCheckboxAndRemainingVisiblity( ) {
            IObservable<bool?> enableTelemetryLogCheckBoxStatus = _enableTelemetryLogCheckBox.GetObservable( CheckBox.IsCheckedProperty );
            _ = enableTelemetryLogCheckBoxStatus.Subscribe( value => _fileNamePanel.IsVisible = value ?? false );
            _ = enableTelemetryLogCheckBoxStatus.Subscribe( value => _logDirectoryPanel.IsVisible = value ?? false );
            _ = enableTelemetryLogCheckBoxStatus.Subscribe( value => _updateLogDirectoryButton.IsVisible = value ?? false );
            _ = enableTelemetryLogCheckBoxStatus.Subscribe( value => _rolloverCountPanel.IsVisible = value ?? false );
            _ = enableTelemetryLogCheckBoxStatus.Subscribe( value => _maximumSizePanel.IsVisible = value ?? false );
        }

        #endregion EnableTelemetryLog Methods


        #region TelemetryLogConfiguration Methods

        #region FileName Methods

        private void AddFileName( string fileName ) {
            _fileNamePanel.Children.Add( _fileNameHeader );
            _fileNamePanel.Children.Add( _fileNameTxt );
            _fileNameTxt.Text = fileName;
            Children.Add( _fileNamePanel );
        }

        #endregion FileName Methods


        #region LogDirectory Methods

        private void AddLogDirectory( string logDirectory ) {
            _logDirectoryPanel.Children.Add( _logDirectoryHeader );
            _logDirectoryPanel.Children.Add( _logDirectoryTxt );
            _logDirectoryTxt.Text = logDirectory;
            _updateLogDirectoryButton.Click += ChangeLogDirHandler;
            Children.Add( _logDirectoryPanel );
            Children.Add( _updateLogDirectoryButton );
        }

        #endregion LogDirectory Methods


        #region RolloverCount Methods

        private void AddRolloverCount( int rollover ) {
            _rolloverCountPanel.Children.Add( _rolloverCountHeader );
            _rolloverCountPanel.Children.Add( _rolloverCount );
            _rolloverCount.Value = rollover;
            Children.Add( _rolloverCountPanel );
        }

        #endregion RolloverCount Methods


        #region MaximumSize Methods

        private void AddMaximumSize( int maximumSize ) {
            _maximumSizePanel.Children.Add( _maximumSizeHeader );
            _maximumSizePanel.Children.Add( _maximumSize );
            _maximumSize.Value = maximumSize;
            Children.Add( _maximumSizePanel );
        }

        #endregion MaximumSize Methods

        #endregion TelemetryLogConfiguration Methods


        #region SaveButton Methods

        private void AddSaveButton( ) {
            _saveButton.Click += SaveConfigHandler;
            Children.Add( _saveButton );
        }

        private Log4NetConfig GetLog4NetConfig( ) {
            _config.EnableTelemetryLog = _enableTelemetryLogCheckBox.IsChecked ?? false;
            _config.TelemetryLogConfiguration = new( );
            _config.TelemetryLogConfiguration.FileName = _fileNameTxt.Text;
            _config.TelemetryLogConfiguration.LogDirectory = _logDirectoryTxt.Text;
            _config.TelemetryLogConfiguration.RolloverCount = (int)_rolloverCount.Value;
            _config.TelemetryLogConfiguration.MaximumSize = (int)_maximumSize.Value;
            return _config;
        }

        #endregion SaveButton Methods


        #region Click Methods

        public async void ChangeLogDirHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            try {
                button.IsEnabled = false;
                OpenFolderDialog dialog = new( ) { Title = "Select Log Directory", };
                string? result = await dialog.ShowAsync( ParentWindow );
                if (result?.Length > 0) { _logDirectoryTxt.Text = result; }
            } catch (Exception ex) {
                await new MessageBox(
                    "Failed to change log directory path.",
                    ex.Message,
                    ex.StackTrace
                ).ShowDialog( );
            } finally {
                button.IsEnabled = true;
            }
        }

        public override async void SaveConfigHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            button.IsEnabled = false;
            try {
                ParentWindow.UpdateConfigSection( GetLog4NetConfig( ) );
                ParentWindow.SetTabContent( this );
                await Task.Delay( 250 );
            } catch (Exception ex) {
                await new MessageBox(
                    "Unable to save sync config.",
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
