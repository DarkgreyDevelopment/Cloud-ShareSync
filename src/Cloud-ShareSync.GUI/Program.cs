using System.CommandLine;
using Avalonia;
using Avalonia.ReactiveUI;
using Cloud_ShareSync.GUI.Types;

namespace Cloud_ShareSync.GUI {
    internal class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static int Main( string[] args ) =>
            new CloudShareSyncGUIRootCommand( ).Invoke( args );

        public static void Start( ) => BuildAvaloniaApp( )
            .StartWithClassicDesktopLifetime( Array.Empty<string>( ) );

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp( )
            => AppBuilder.Configure<App>( )
                .UsePlatformDetect( )
                .LogToTrace( )
                .UseReactiveUI( );
    }
}
