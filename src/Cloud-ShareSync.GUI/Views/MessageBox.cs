using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ReactiveUI;

namespace Cloud_ShareSync.GUI.Views {

    public partial class MessageBox : Window {

        public MessageBox(
            string title,
            string text,
            string? stackTraceMsg = null
        ) {
            _text = text;
            _stackTrace = stackTraceMsg;
            ConfigureWindowProperties( title );
            ConfigureMainPanel( );
            Content = _panel;
        }

        #region Fields

        private readonly string _text;
        private readonly string? _stackTrace;

        private readonly StackPanel _panel = new( ) {
            HorizontalAlignment = HorizontalAlignment.Center
        };

        private readonly StackPanel _buttonPanel = new( ) {
            Name = "ButtonPanel",
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Horizontal,
        };

        private readonly TextBlock _errorText = new( ) {
            Name = "ErrorText",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = Thickness.Parse( "10,5,10,5" )
        };

        private readonly Button _okButton = new( ) {
            Name = "OkButton",
            Content = "Ok",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        private readonly Button _stackTraceButton = new( ) {
            Name = "StackTraceButton",
            Content = "Show StackTrace",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        #endregion Fields


        #region Configure Window

        private void ConfigureWindowProperties( string title ) {
            Title = _stackTrace != null ? $"Error - {title}" : title;
            DataContext = new ReactiveObject( );
            Icon = App.Icon;
            SizeToContent = SizeToContent.WidthAndHeight;
            CanResize = false;
        }

        public async Task ShowDialog( ) {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                await ShowDialog( desktop.MainWindow );
            }
        }

        #endregion Configure Window


        #region Configure Panel

        private void ConfigureMainPanel( ) {
            _errorText.Text = _text;
            _panel.Children.Add( _errorText );
            ConfigureButtonPanel( );
            _panel.Children.Add( _buttonPanel );
        }

        private void ConfigureButtonPanel( ) {
            ConfigureClickActions( );
            ConfigureButtonPanelChildren( );
        }

        private void ConfigureButtonPanelChildren( ) {
            _buttonPanel.Children.Add( _okButton );
            if (_stackTrace != null) {
                _buttonPanel.Children.Add( _stackTraceButton );
            }
        }

        #endregion Configure Panel


        #region ClickActions

        private void ConfigureClickActions( ) {
            _okButton.Click += ClickOk;
            _stackTraceButton.Click += ClickShowStackTrace;
        }

        private void ClickOk( object? sender, RoutedEventArgs e ) => Close( );

        private void ClickShowStackTrace( object? sender, RoutedEventArgs e ) {
            Button btn = (sender as Button)!;
            if ((btn.Content as string) == "Show StackTrace") {
                btn.Content = "Hide StackTrace";
                _errorText.Text = _text + ((_stackTrace == null) ? "\nnull" : $"\n{_stackTrace}");
            } else {
                btn.Content = "Show StackTrace";
                _errorText.Text = _text;
            }
        }

        #endregion ClickActions

    }
}
