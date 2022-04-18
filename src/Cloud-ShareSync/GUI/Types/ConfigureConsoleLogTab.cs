using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Cloud_ShareSync.Configuration.Types;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal class ConfigureConsoleLogTab : StackPanel, IConfigurationTab {

        public ConfigureConsoleLogTab( Log4NetConfig config, ConfigureWindow parent ) {
            _parentWindow = parent;
            _config = config;
            _logLevels = new( config.ConsoleConfiguration?.LogLevels ?? 0 );
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


        private readonly LogLevelsSection _logLevels;

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
            Children.Add( _logLevels );
            AddSaveButton( );
            LinkEnableCheckboxAndRemainingVisiblity( );
        }

        private void AddEnableConsoleLogCheckBox( bool state ) {
            _enableConsoleLogCheckBox.IsChecked = state;
            Children.Add( _enableConsoleLogCheckBox );
        }

        private void LinkEnableCheckboxAndRemainingVisiblity( ) {
            IObservable<bool?> enableConsoleCheckBoxStatus = _enableConsoleLogCheckBox.GetObservable( CheckBox.IsCheckedProperty );
            _ = enableConsoleCheckBoxStatus.Subscribe( value => _useStdErrCheckBox.IsVisible = value ?? false );
            _ = enableConsoleCheckBoxStatus.Subscribe( value => _enableColoredConsoleCheckBox.IsVisible = value ?? false );
            _ = enableConsoleCheckBoxStatus.Subscribe( value => _logLevels.IsVisible = value ?? false );
        }

        private void AddUseStdErrCheckBox( bool state ) {
            _useStdErrCheckBox.IsChecked = state;
            Children.Add( _useStdErrCheckBox );
        }

        private void AddEnableColoredConsoleCheckBox( bool state ) {
            _enableColoredConsoleCheckBox.IsChecked = state;
            Children.Add( _enableColoredConsoleCheckBox );
        }

        private void AddSaveButton( ) {
            _saveButton.Click += SaveConfigHandler;
            Children.Add( _saveButton );
        }

        private Log4NetConfig GetLog4NetConfig( ) {
            _config.EnableConsoleLog = _enableConsoleLogCheckBox.IsChecked ?? false;
            _config.ConsoleConfiguration = new( );
            _config.ConsoleConfiguration.UseStdErr = _useStdErrCheckBox.IsChecked ?? false;
            _config.ConsoleConfiguration.EnableColoredConsole = _enableColoredConsoleCheckBox.IsChecked ?? false;
            _config.ConsoleConfiguration.LogLevels = _logLevels.GetSupportedLogLevels( );
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

        #endregion Methods

    }

}
