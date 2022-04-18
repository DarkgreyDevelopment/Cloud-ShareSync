using Cloud_ShareSync.Core.Logging.Telemetry;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;

namespace Cloud_ShareSync.Core.Logging.Appenders.Extensions {
    internal static class LogAppenderExtensions {

        private const string LogMessageFormat =
            "%utcdate{yyyy-MM-dd}T%utcdate{HH:mm:ss.ffff}Z [%thread] %5level: %m%n";

        public static PatternLayout DefaultPatternLayout( this PatternLayout patternLayout ) {
            patternLayout.ConversionPattern = LogMessageFormat;
            patternLayout.ActivateOptions( );
            return patternLayout;
        }

        public static void AddFilters( this AppenderSkeleton appender, List<IFilter> list ) {
            foreach (IFilter filter in list) { appender.AddFilter( filter ); }
        }

        public static List<IFilter> CreateFiltersList( this Level[] requestedLogLevels, bool addErrorsLevels ) {
            // Create an Accept LevelMatchFilter for all specified LogLevels.
            List<IFilter> filterList = new( );
            foreach (Level level in requestedLogLevels) {
                if (addErrorsLevels == false && (level == Level.Fatal || level == Level.Error)) { continue; }
                filterList.Add(
                    new LevelMatchFilter {
                        LevelToMatch = level,
                        AcceptOnMatch = true
                    }
                );
            }

            // Create Default Deny Filter to exclude all non-specified LogLevels.
            filterList.Add( new DenyAllFilter( ) );
            return filterList;
        }

        private const SupportedLogLevels AllLevels =
            SupportedLogLevels.Fatal |
            SupportedLogLevels.Error |
            SupportedLogLevels.Warn |
            SupportedLogLevels.Info |
            SupportedLogLevels.Debug |
            SupportedLogLevels.Telemetry;

        public static Level[] TranslateLogLevel( this SupportedLogLevels logLevels, bool addErrorsLevels ) {
            if (logLevels == AllLevels) { return Array.Empty<Level>( ); } // Don't filter if we want all levels included.
            List<Level> levels = new( );
            AddFatalLevel( logLevels, levels, addErrorsLevels );
            AddErrorLevel( logLevels, levels, addErrorsLevels );
            AddWarnLevel( logLevels, levels );
            AddInfoLevel( logLevels, levels );
            AddDebugLevel( logLevels, levels );
            AddTelemetryLevel( logLevels, levels );
            return levels.ToArray( );
        }

        private static void AddFatalLevel( SupportedLogLevels logLevels, List<Level> levels, bool addErrorsLevels ) {
            if (addErrorsLevels && logLevels.HasFlag( SupportedLogLevels.Fatal )) { levels.Add( Level.Fatal ); }
        }

        private static void AddErrorLevel( SupportedLogLevels logLevels, List<Level> levels, bool addErrorsLevels ) {
            if (addErrorsLevels && logLevels.HasFlag( SupportedLogLevels.Error )) { levels.Add( Level.Error ); }
        }

        private static void AddWarnLevel( SupportedLogLevels logLevels, List<Level> levels ) {
            if (logLevels.HasFlag( SupportedLogLevels.Warn )) { levels.Add( Level.Warn ); }
        }

        private static void AddInfoLevel( SupportedLogLevels logLevels, List<Level> levels ) {
            if (logLevels.HasFlag( SupportedLogLevels.Info )) { levels.Add( Level.Info ); }
        }

        private static void AddDebugLevel( SupportedLogLevels logLevels, List<Level> levels ) {
            if (logLevels.HasFlag( SupportedLogLevels.Debug )) { levels.Add( Level.Debug ); }
        }

        private static void AddTelemetryLevel( SupportedLogLevels logLevels, List<Level> levels ) {
            if (logLevels.HasFlag( SupportedLogLevels.Telemetry )) {
                levels.Add( TelemetryLogLevelExtension.TelemetryLevel );
            }
        }

    }
}
