using Cloud_ShareSync.Core.Logging.Appenders.Extensions;
using log4net.Appender;
using log4net.Layout;

namespace Cloud_ShareSync.Core.Logging.Appenders {
    internal class ConsoleLogAppender : ConsoleAppender {

        public ConsoleLogAppender(
            SupportedLogLevels logLevels,
            bool addErrorsLevels
        ) {
            try {
                this.AddFilters(
                    logLevels
                    .TranslateLogLevel( addErrorsLevels )
                    .CreateFiltersList( addErrorsLevels )
                );
                Layout = new PatternLayout( ).DefaultPatternLayout( );
                ActivateOptions( );
            } catch (Exception consoleLogFailure) {
                Console.WriteLine(
                    "Failed to add Log4Net ConsoleAppender. Console logging will be limited.\n" +
                    consoleLogFailure.ToString( )
                );
            }
        }
    }
}
