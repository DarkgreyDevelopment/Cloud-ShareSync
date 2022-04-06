using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ReactiveUI;

namespace Cloud_ShareSync.GUI.Views {
    // https://stackoverflow.com/a/55707749
    public partial class ErrorDialog : Window {

        public ErrorDialog(
            string title,
            string text,
            string? stackTraceMsg = null
        ) {
            _text = text;
            _stackTrace = stackTraceMsg;
            ConfigureWindowProperties( title );
            ConfigureMainPanel( );
            Content = Panel;
        }

        #region Fields

        private readonly string _text;
        private readonly string? _stackTrace;

        public StackPanel Panel { get; } = new( ) {
            HorizontalAlignment = HorizontalAlignment.Center
        };

        public StackPanel ButtonPanel { get; } = new( ) {
            Name = "ButtonPanel",
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Horizontal,
        };

        public TextBlock ErrorText { get; } = new( ) {
            Name = "ErrorText",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = Thickness.Parse( "10,5,10,5" )
        };

        public Button OkButton { get; } = new( ) {
            Name = "OkButton",
            Content = "Ok",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        public Button StackTraceButton { get; } = new( ) {
            Name = "StackTraceButton",
            Content = "Show StackTrace",
            Margin = Thickness.Parse( "5,5,5,5" )
        };

        #endregion Fields

        private void ConfigureMainPanel( ) {
            ErrorText.Text = _text;
            Panel.Children.Add( ErrorText );
            ConfigureButtonPanel( );
            Panel.Children.Add( ButtonPanel );
        }

        private void ConfigureButtonPanel( ) {
            ConfigureClickActions( );
            ConfigureButtonPanelChildren( );
        }

        private void ConfigureButtonPanelChildren( ) {
            ButtonPanel.Children.Add( OkButton );
            if (_stackTrace != null) {
                ButtonPanel.Children.Add( StackTraceButton );
            }
        }

        private void ConfigureClickActions( ) {
            OkButton.Click += ClickOk;
            StackTraceButton.Click += ClickShowStackTrace;
        }

        private void ConfigureWindowProperties( string title ) {
            Title = $"Error - {title}";
            DataContext = new ReactiveObject( );
            Icon = new WindowIcon(
                MainWindow.AssetLoader?.Open(
                        new Uri( @"resm:Cloud_ShareSync.GUI.Assets.logo.ico" )
                )
            );
            SizeToContent = SizeToContent.WidthAndHeight;
            CanResize = false;
        }

        public async Task ShowDialog( ) {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                await ShowDialog( desktop.MainWindow );
            }
        }

        private void ClickOk( object? sender, RoutedEventArgs e ) => Close( );

        private void ClickShowStackTrace( object? sender, RoutedEventArgs e ) {
            Button btn = (sender as Button)!;
            if ((btn.Content as string) == "Show StackTrace") {
                btn.Content = "Hide StackTrace";
                ErrorText.Text = _text + ((_stackTrace == null) ? "\nnull" : $"\n{_stackTrace}");
            } else {
                btn.Content = "Show StackTrace";
                ErrorText.Text = _text;
            }
        }
    }
}
