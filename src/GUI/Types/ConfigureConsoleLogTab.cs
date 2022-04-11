using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureConsoleLogTab : StackPanel, IConfigurationTab {

        public ConfigureConsoleLogTab( Log4NetConfig config, ConfigureWindow parent ) {
            _parentWindow = parent;
            _config = config;
            ConfigureWindowSettings( );
            ConfigureWindowContent( config );
        }


        #region Fields

        private readonly Log4NetConfig _config;

        private readonly ConfigureWindow _parentWindow;

        private readonly Button _saveButton = new( ) {
            Name = "SaveConsoleLogConfig",
            Margin = Thickness.Parse( "5,15,5,15" ),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = true,
            Content = "Save"
        };

        private readonly CheckBox _enableConsoleLogCheckBox = new( ) {
            IsChecked = true,
            Content = "Enable Console Log",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        private readonly CheckBox _useStdErrCheckBox = new( ) {
            IsChecked = true,
            Content = "Use StdErr",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        private readonly CheckBox _enableColoredConsoleCheckBox = new( ) {
            IsChecked = true,
            Content = "Enable Colored Console",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

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
            Name = "ConfigureConsoleLogTab";
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;
        }

        private void ConfigureWindowContent( Log4NetConfig config ) {
            AddEnableConsoleLogCheckBox( config.EnableConsoleLog );
            AddUseStdErrCheckBox( config.ConsoleConfiguration?.UseStdErr ?? false );
            AddEnableColoredConsoleCheckBox( config.ConsoleConfiguration?.EnableColoredConsole ?? false );
            Children.Add( _logLevelsHeader );
            AddLogLevelsPanel( );
            SetLogLevelCheckBoxStatus( config.ConsoleConfiguration?.LogLevels ?? 0 );
            AddSaveButton( );
            LinkEnableCheckboxAndRemainingVisiblity( );
        }

        #region EnableConsoleLog Methods

        private void AddEnableConsoleLogCheckBox( bool state ) {
            _enableConsoleLogCheckBox.IsChecked = state;
            Children.Add( _enableConsoleLogCheckBox );
        }

        private void LinkEnableCheckboxAndRemainingVisiblity( ) {
            IObservable<bool?> enableConsoleCheckBoxStatus = _enableConsoleLogCheckBox.GetObservable( CheckBox.IsCheckedProperty );
            _ = enableConsoleCheckBoxStatus.Subscribe( value => _useStdErrCheckBox.IsVisible = value ?? false );
            _ = enableConsoleCheckBoxStatus.Subscribe( value => _enableColoredConsoleCheckBox.IsVisible = value ?? false );
            _ = enableConsoleCheckBoxStatus.Subscribe( value => _logLevelsHeader.IsVisible = value ?? false );
            _ = enableConsoleCheckBoxStatus.Subscribe( value => _logLevelsPanel.IsVisible = value ?? false );
        }

        #endregion EnableConsoleLog Methods


        #region ConsoleConfiguration Methods

        private void AddUseStdErrCheckBox( bool state ) {
            _useStdErrCheckBox.IsChecked = state;
            Children.Add( _useStdErrCheckBox );
        }

        private void AddEnableColoredConsoleCheckBox( bool state ) {
            _enableColoredConsoleCheckBox.IsChecked = state;
            Children.Add( _enableColoredConsoleCheckBox );
        }

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

        #endregion ConsoleConfiguration Methods


        #region SaveButton Methods

        private void AddSaveButton( ) {
            _saveButton.Click += SaveConfigHandler;
            Children.Add( _saveButton );
        }

        private Log4NetConfig GetLog4NetConfig( ) {
            _config.EnableConsoleLog = _enableConsoleLogCheckBox.IsChecked ?? false;
            _config.ConsoleConfiguration = new( );
            _config.ConsoleConfiguration.UseStdErr = _useStdErrCheckBox.IsChecked ?? false;
            _config.ConsoleConfiguration.EnableColoredConsole = _enableColoredConsoleCheckBox.IsChecked ?? false;
            _config.ConsoleConfiguration.LogLevels = GetLogLevelCheckBoxStatus( );
            return _config;
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

        #endregion SaveButton Methods

        #endregion Methods

    }

}
