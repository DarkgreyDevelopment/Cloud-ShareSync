using Cloud_ShareSync.Core.Logging.Appenders.Extensions;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace Cloud_ShareSync.Core.Logging.Appenders {
    internal class ColoredConsoleErrorLogAppender : ColoredConsoleAppender {

        public static readonly Level[] ErrLvl = new Level[] { Level.Fatal, Level.Error };

        public ColoredConsoleErrorLogAppender( SupportedLogLevels logLevels ) {
            try {
                this.AddErrorFilters( logLevels );
                this.AddMappings( ErrLvl.CreateColorMappingsList( ) );
                this.RegisterCodePage( );
                Layout = new PatternLayout( ).DefaultPatternLayout( );
                Target = "Console.Error";
                ActivateOptions( );
            } catch (Exception consoleLogFailure) {
                Console.Error.WriteLine(
                    "Failed to add Log4Net ColoredConsoleErrorAppender. Console logging will be limited.\n" +
                    consoleLogFailure.ToString( )
                );
            }
        }
    }
}
