using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Cloud_ShareSync.GUI.Views {
    // https://stackoverflow.com/a/55707749
    public partial class ErrorDialog : Window {

        public ErrorDialog( ) { AvaloniaXamlLoader.Load( this ); }

        public static Task Show( string title, string text, string? stackTraceMsg = null ) {
            Console.WriteLine( title );
            ErrorDialog errDialog = NewErrorDialog( title, text );
            AddButtons( errDialog, text, stackTraceMsg );
            errDialog.Show( );
            return Task.CompletedTask;
        }

        internal static ErrorDialog NewErrorDialog( string title, string text ) {
            ErrorDialog errDialog = new( ) { Title = $"Error - {title}" };
            errDialog.FindControl<TextBlock>( "Text" ).Text = text;
            return errDialog;
        }

        internal static void AddButtons(
            ErrorDialog errDialog,
            string text,
            string? stackTraceMsg
        ) {
            StackPanel buttonPanel = errDialog.FindControl<StackPanel>( "Buttons" );
            AddOkButton( errDialog, buttonPanel );
            AddStackTraceButton( errDialog, buttonPanel, text, stackTraceMsg );
        }

        internal static void AddOkButton(
            ErrorDialog errDialog,
            StackPanel buttonPanel
        ) {
            Button btn = new( ) { Content = "Ok" };
            btn.Click += ( _, _ ) => errDialog.Close( );
            buttonPanel.Children.Add( btn );
        }

        internal static void AddStackTraceButton(
            ErrorDialog errDialog,
            StackPanel buttonPanel,
            string text,
            string? stackTraceMsg
        ) {
            Button stackTrace = new( ) { Content = "Show StackTrace" };
            stackTrace.Click += ( _, _ ) => {
                if ((stackTrace.Content as string) == "Show StackTrace") {
                    stackTrace.Content = "Hide StackTrace";
                    errDialog.FindControl<TextBlock>( "Text" ).Text = text + ((stackTraceMsg == null) ? "\nnull" : $"\n{stackTraceMsg}");
                } else {
                    stackTrace.Content = "Show StackTrace";
                    errDialog.FindControl<TextBlock>( "Text" ).Text = text;
                }
            };
            buttonPanel.Children.Add( stackTrace );
        }
    }
}
