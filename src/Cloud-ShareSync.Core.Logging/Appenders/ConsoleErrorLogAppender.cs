using Cloud_ShareSync.Core.Logging.Appenders.Extensions;
using log4net.Appender;
using log4net.Layout;

namespace Cloud_ShareSync.Core.Logging.Appenders {
    internal class ConsoleErrorLogAppender : ConsoleAppender {

        public ConsoleErrorLogAppender( SupportedLogLevels logLevels ) {
            try {
                this.AddErrorFilters( logLevels );
                Layout = new PatternLayout( ).DefaultPatternLayout( );
                Target = "Console.Error";
                ActivateOptions( );
            } catch (Exception consoleLogFailure) {
                Console.Error.WriteLine(
                    "Failed to add Log4Net ConsoleErrorAppender. Console logging will be limited.\n" +
                    consoleLogFailure.ToString( )
                );
            }
        }
    }
}
