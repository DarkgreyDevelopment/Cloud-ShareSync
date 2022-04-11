using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureDefaultLogTab : StackPanel, IConfigurationTab {

        public ConfigureDefaultLogTab( Log4NetConfig config, ConfigureWindow parent ) {
            _parentWindow = parent;
            _config = config;
            ConfigureWindowSettings( );
            ConfigureWindowContent( config );
        }


        #region Fields

        private readonly Log4NetConfig _config;

        private readonly ConfigureWindow _parentWindow;

        private readonly Button _saveButton = new( ) {
            Name = "SaveDefaultLogConfig",
            Margin = Thickness.Parse( "5,15,5,15" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Save"
        };

        private readonly CheckBox _enableDefaultLogCheckBox = new( ) {
            IsChecked = true,
            Content = "Enable Default Log",
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


        #region LogLevels

        private readonly TextBlock _logLevelsHeader = new( ) {
            Text = "Log Levels: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
        };

        private readonly WrapPanel _logLevelsPanel = new( ) {
            Margin = Thickness.Parse( "5,0,5,15" )
        };

        private readonly CheckBox _fatalCheckBox = new( ) {
            IsChecked = true,
            Content = "Fatal",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        private readonly CheckBox _errorCheckBox = new( ) {
            IsChecked = true,
            Content = "Error",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        private readonly CheckBox _warnCheckBox = new( ) {
            IsChecked = true,
            Content = "Warning",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        private readonly CheckBox _infoCheckBox = new( ) {
            IsChecked = false,
            Content = "Information",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        private readonly CheckBox _debugCheckBox = new( ) {
            IsChecked = false,
            Content = "Debug",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        private readonly CheckBox _telemetryCheckBox = new( ) {
            IsChecked = false,
            Content = "Telemetry",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        #endregion LogLevels

        #endregion Fields


        #region Methods

        private void ConfigureWindowSettings( ) {
            Name = "ConfigureDefaultLogTab";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;
        }

        private void ConfigureWindowContent( Log4NetConfig config ) {
            AddEnableDefaultLogCheckBox( config.EnableDefaultLog );
            AddFileName( config.DefaultLogConfiguration?.FileName ?? "" );
            AddLogDirectory( config.DefaultLogConfiguration?.LogDirectory ?? "" );
            AddRolloverCount( config.DefaultLogConfiguration?.RolloverCount ?? 0 );
            AddMaximumSize( config.DefaultLogConfiguration?.MaximumSize ?? 1 );
            Children.Add( _logLevelsHeader );
            AddLogLevelsPanel( );
            SetLogLevelCheckBoxStatus( config.DefaultLogConfiguration?.LogLevels ?? 0 );
            AddSaveButton( );
            LinkEnableCheckboxAndRemainingVisiblity( );
        }

        #region EnableDefaultLog Methods

        private void AddEnableDefaultLogCheckBox( bool state ) {
            _enableDefaultLogCheckBox.IsChecked = state;
            Children.Add( _enableDefaultLogCheckBox );
        }

        private void LinkEnableCheckboxAndRemainingVisiblity( ) {
            IObservable<bool?> enableDefaultLogCheckBoxStatus = _enableDefaultLogCheckBox.GetObservable( CheckBox.IsCheckedProperty );
            _ = enableDefaultLogCheckBoxStatus.Subscribe( value => _fileNamePanel.IsVisible = value ?? false );
            _ = enableDefaultLogCheckBoxStatus.Subscribe( value => _logDirectoryPanel.IsVisible = value ?? false );
            _ = enableDefaultLogCheckBoxStatus.Subscribe( value => _updateLogDirectoryButton.IsVisible = value ?? false );
            _ = enableDefaultLogCheckBoxStatus.Subscribe( value => _rolloverCountPanel.IsVisible = value ?? false );
            _ = enableDefaultLogCheckBoxStatus.Subscribe( value => _maximumSizePanel.IsVisible = value ?? false );
            _ = enableDefaultLogCheckBoxStatus.Subscribe( value => _logLevelsHeader.IsVisible = value ?? false );
            _ = enableDefaultLogCheckBoxStatus.Subscribe( value => _logLevelsPanel.IsVisible = value ?? false );
        }

        #endregion EnableDefaultLog Methods


        #region DefaultLogConfiguration Methods


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


        #region LogLevels Methods

        private void AddLogLevelsPanel( ) {
            _logLevelsPanel.Children.Add( _fatalCheckBox );
            _logLevelsPanel.Children.Add( _errorCheckBox );
            _logLevelsPanel.Children.Add( _warnCheckBox );
            _logLevelsPanel.Children.Add( _infoCheckBox );
            _logLevelsPanel.Children.Add( _debugCheckBox );
            _logLevelsPanel.Children.Add( _telemetryCheckBox );
            Children.Add( _logLevelsPanel );
        }

        private void SetLogLevelCheckBoxStatus( SupportedLogLevels logLevels ) {
            _fatalCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Fatal );
            _errorCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Error );
            _warnCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Warn );
            _infoCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Info );
            _debugCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Debug );
            _telemetryCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Telemetry );
        }

        private SupportedLogLevels GetLogLevelCheckBoxStatus( ) {
            SupportedLogLevels logLevels = 0;
            if (_fatalCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Fatal; }
            if (_errorCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Error; }
            if (_warnCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Warn; }
            if (_infoCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Info; }
            if (_debugCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Debug; }
            if (_telemetryCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Telemetry; }
            return logLevels;
        }

        #endregion LogLevels Methods

        #endregion DefaultLogConfiguration Methods


        #region SaveButton Methods

        private void AddSaveButton( ) {
            _saveButton.Click += SaveConfigHandler;
            Children.Add( _saveButton );
        }

        private Log4NetConfig GetLog4NetConfig( ) {
            _config.EnableDefaultLog = _enableDefaultLogCheckBox.IsChecked ?? false;
            _config.DefaultLogConfiguration = new( );
            _config.DefaultLogConfiguration.FileName = _fileNameTxt.Text;
            _config.DefaultLogConfiguration.LogDirectory = _logDirectoryTxt.Text;
            _config.DefaultLogConfiguration.RolloverCount = (int)_rolloverCount.Value;
            _config.DefaultLogConfiguration.MaximumSize = (int)_maximumSize.Value;
            _config.DefaultLogConfiguration.LogLevels = GetLogLevelCheckBoxStatus( );
            return _config;
        }

        #endregion SaveButton Methods


        #region Click Methods

        public async void ChangeLogDirHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            try {
                button.IsEnabled = false;
                OpenFolderDialog dialog = new( ) { Title = "Select Log Directory", };
                string? result = await dialog.ShowAsync( _parentWindow );
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

        public async void SaveConfigHandler( object? sender, RoutedEventArgs e ) {
            Button button = (sender as Button)!;
            button.IsEnabled = false;
            try {
                _parentWindow.UpdateConfigSection( GetLog4NetConfig( ) );
                _parentWindow.SetTabContent( this );
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
