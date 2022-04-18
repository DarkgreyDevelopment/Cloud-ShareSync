using System.Text;
using log4net.Appender;

namespace Cloud_ShareSync.Core.Logging.Appenders.Extensions {
    internal static class ConsoleAppenderExtensions {

        public static void RegisterCodePage( this ColoredConsoleAppender consoleAppender ) {
            try {
                // Register terminal code page for console output.
                Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );
            } catch (Exception ex) {
                Console.Error.WriteLine(
                    "Failed to register console codepage. Console logging may be limited. Error:\n" +
                    ex.ToString( )
                );
            }
        }

    }
}
