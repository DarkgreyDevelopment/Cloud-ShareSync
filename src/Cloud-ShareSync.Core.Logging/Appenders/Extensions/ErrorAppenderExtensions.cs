using log4net.Appender;
using log4net.Core;

namespace Cloud_ShareSync.Core.Logging.Appenders.Extensions {
    internal static class ErrorAppenderExtensions {

        public static void AddErrorFilters( this AppenderSkeleton consoleErrorAppender, SupportedLogLevels logLevels ) {
            if (logLevels.HasFlag( SupportedLogLevels.Fatal )) {
                consoleErrorAppender.AddFilter(
                    new log4net.Filter.LevelMatchFilter {
                        LevelToMatch = Level.Fatal,
                        AcceptOnMatch = true
                    }
                );
            }

            if (logLevels.HasFlag( SupportedLogLevels.Error )) {
                consoleErrorAppender.AddFilter(
                    new log4net.Filter.LevelMatchFilter {
                        LevelToMatch = Level.Error,
                        AcceptOnMatch = true
                    }
                );
            }
            consoleErrorAppender.AddFilter( new log4net.Filter.DenyAllFilter( ) );
        }

    }
}
