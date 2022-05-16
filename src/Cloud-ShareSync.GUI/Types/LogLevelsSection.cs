using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Cloud_ShareSync.Core.Logging;

namespace Cloud_ShareSync.GUI.Types {
    internal class LogLevelsSection : WrapPanel {
        public LogLevelsSection( SupportedLogLevels logLevels ) {
            SetLogLevelCheckBoxStatus( logLevels );
            Children.Add( _logLevelsHeader );
            Children.Add( ConfigureCheckboxGrid( ) );
        }

        #region Fields

        private readonly TextBlock _logLevelsHeader = new( ) {
            Text = "Log Levels: ",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 16
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

        #endregion Fields


        #region Methods

        public SupportedLogLevels GetSupportedLogLevels( ) {
            SupportedLogLevels logLevels = 0;
            if (_fatalCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Fatal; }
            if (_errorCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Error; }
            if (_warnCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Warn; }
            if (_infoCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Info; }
            if (_debugCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Debug; }
            if (_telemetryCheckBox.IsChecked == true) { logLevels |= SupportedLogLevels.Telemetry; }
            return logLevels;
        }

        private void SetLogLevelCheckBoxStatus( SupportedLogLevels logLevels ) {
            _fatalCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Fatal );
            _errorCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Error );
            _warnCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Warn );
            _infoCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Info );
            _debugCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Debug );
            _telemetryCheckBox.IsChecked = logLevels.HasFlag( SupportedLogLevels.Telemetry );
        }

        #region Configure Checkbox Grid

        private Grid ConfigureCheckboxGrid( ) {
            Grid grid = NewCheckboxGrid( );
            SetCheckboxGridPositions( grid );
            return grid;
        }

        private static Grid NewCheckboxGrid( ) => new( ) {
            Margin = Thickness.Parse( "5,0,5,0" ),
            RowDefinitions = new RowDefinitions( ) {
                new RowDefinition(){ Height = GridLength.Star }
            },
            ColumnDefinitions = new ColumnDefinitions( ) {
                new ColumnDefinition(){ Width = GridLength.Star },
                new ColumnDefinition(){ Width = GridLength.Star },
                new ColumnDefinition(){ Width = GridLength.Star },
                new ColumnDefinition(){ Width = GridLength.Star },
                new ColumnDefinition(){ Width = GridLength.Star },
                new ColumnDefinition(){ Width = GridLength.Star },
                new ColumnDefinition(){ Width = GridLength.Star }
            }
        };

        private void SetCheckboxGridPositions( Grid grid ) {
            SetFatalCheckboxGridPosition( grid );
            SetErrorCheckboxGridPosition( grid );
            SetWarnCheckboxGridPosition( grid );
            SetInfoCheckboxGridPosition( grid );
            SetDebugCheckboxGridPosition( grid );
            SetTelemetryCheckboxGridPosition( grid );
        }

        private void SetFatalCheckboxGridPosition( Grid grid ) {
            Grid.SetColumn( _fatalCheckBox, 0 );
            Grid.SetRow( _fatalCheckBox, 0 );
            grid.Children.Add( _fatalCheckBox );
        }

        private void SetErrorCheckboxGridPosition( Grid grid ) {
            Grid.SetColumn( _errorCheckBox, 1 );
            Grid.SetRow( _errorCheckBox, 0 );
            grid.Children.Add( _errorCheckBox );
        }

        private void SetWarnCheckboxGridPosition( Grid grid ) {
            Grid.SetColumn( _warnCheckBox, 2 );
            Grid.SetRow( _warnCheckBox, 0 );
            grid.Children.Add( _warnCheckBox );
        }

        private void SetInfoCheckboxGridPosition( Grid grid ) {
            Grid.SetColumn( _infoCheckBox, 3 );
            Grid.SetRow( _infoCheckBox, 0 );
            grid.Children.Add( _infoCheckBox );
        }

        private void SetDebugCheckboxGridPosition( Grid grid ) {
            Grid.SetColumn( _debugCheckBox, 4 );
            Grid.SetRow( _debugCheckBox, 0 );
            grid.Children.Add( _debugCheckBox );
        }

        private void SetTelemetryCheckboxGridPosition( Grid grid ) {
            Grid.SetColumn( _telemetryCheckBox, 5 );
            Grid.SetRow( _telemetryCheckBox, 0 );
            grid.Children.Add( _telemetryCheckBox );
        }

        #endregion Configure Checkbox Grid

        #endregion Methods
    }
}
