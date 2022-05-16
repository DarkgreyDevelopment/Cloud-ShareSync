using Cloud_ShareSync.Core.Logging.Appenders.Extensions;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace Cloud_ShareSync.Core.Logging.Appenders {
    internal class ColoredConsoleLogAppender : ColoredConsoleAppender {

        public ColoredConsoleLogAppender(
            SupportedLogLevels logLevels,
            bool addErrorsLevels
        ) {
            try {
                Level[] levels = logLevels.TranslateLogLevel( addErrorsLevels );
                this.AddFilters( levels.CreateFiltersList( addErrorsLevels ) );
                this.AddMappings( levels.CreateColorMappingsList( ) );
                ConsoleAppenderExtensions.RegisterCodePage( );
                Layout = new PatternLayout( ).DefaultPatternLayout( );
                ActivateOptions( );
            } catch (Exception consoleLogFailure) {
                Console.WriteLine(
                    "Failed to add Log4Net ColoredConsoleAppender. Console logging will be limited.\n" +
                    consoleLogFailure.ToString( )
                );
            }
        }
    }
}
