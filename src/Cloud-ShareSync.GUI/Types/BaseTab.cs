using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Cloud_ShareSync.GUI.Views;

namespace Cloud_ShareSync.GUI.Types {
    internal abstract class BaseTab : StackPanel, IConfigurationTab {
        public BaseTab( ConfigureWindow parent ) {
            ParentWindow = parent;
        }
        internal ConfigureWindow ParentWindow { get; set; }
        internal void ConfigureWindowSettings( ) {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;
        }
        public abstract void SaveConfigHandler( object? sender, RoutedEventArgs e );
    }
}
