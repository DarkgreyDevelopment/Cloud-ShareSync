using Avalonia;
using Avalonia.Controls;

namespace Cloud_ShareSync.GUI.Types {
    internal class UpdatePathButton : Button {

        public static readonly StyledProperty<string?> PathProperty =
            AvaloniaProperty.Register<UpdatePathButton, string?>( nameof( Path ) );

        public string? Path {
            get { return GetValue( PathProperty ); }
            set { _ = SetValue( PathProperty, value ); }
        }
    }
}
