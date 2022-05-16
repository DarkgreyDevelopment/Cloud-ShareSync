using Cloud_ShareSync.Core.Logging.Telemetry;
using log4net.Appender;
using log4net.Core;

namespace Cloud_ShareSync.Core.Logging.Appenders.Extensions {
    internal static class ColoredConsoleAppenderExtensions {

        public static void AddMappings(
            this ColoredConsoleAppender appender,
            List<ColoredConsoleAppender.LevelColors> mappings
        ) {
            foreach (ColoredConsoleAppender.LevelColors mapping in mappings) {
                appender.AddMapping( mapping );
            }
        }

        public static List<ColoredConsoleAppender.LevelColors> CreateColorMappingsList( this Level[] requestedLogLevels ) {
            List<ColoredConsoleAppender.LevelColors> map = new( );
            foreach (Level level in requestedLogLevels) {
                GetFatalColor( level, map );
                GetErrorColor( level, map );
                GetWarnColor( level, map );
                GetInfoColor( level, map );
                GetDebugColor( level, map );
                GetTelemetryColor( level, map );
            }
            return map;
        }

        private static void GetFatalColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == Level.Fatal) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.White,
                        BackColor = ColoredConsoleAppender.Colors.Red,
                        Level = Level.Fatal
                    }
                );
            }
        }

        private static void GetErrorColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == Level.Error) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.Red,
                        Level = Level.Error
                    }
                );
            }
        }

        private static void GetWarnColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == Level.Warn) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.Yellow,
                        Level = Level.Warn
                    }
                );
            }
        }

        private static void GetInfoColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == Level.Info) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.Green,
                        Level = Level.Info
                    }
                );
            }
        }

        private static void GetDebugColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == Level.Debug) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.Blue,
                        Level = Level.Debug
                    }
                );
            }
        }

        private static void GetTelemetryColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == TelemetryLogLevelExtension.TelemetryLevel) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.White,
                        Level = TelemetryLogLevelExtension.TelemetryLevel
                    }
                );
            }
        }

    }
}
