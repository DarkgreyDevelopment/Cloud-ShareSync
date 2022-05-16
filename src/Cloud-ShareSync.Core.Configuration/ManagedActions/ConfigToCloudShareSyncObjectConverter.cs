using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Configuration.ManagedActions {
    public static class ConfigToCloudShareSyncObjectConverter {

        public static ILogger ConvertLog4NetConfigToILogger(
            Log4NetConfig? config,
            string[] oTelSources
        ) {
            TelemetryLogger result = new( "Cloud-ShareSync", oTelSources );
            if (config != null) {
                ConfigureTelemetryLogger( config, result );
            }
            result.SetConfigured( );
            return result;
        }

        private static void ConfigureTelemetryLogger(
            Log4NetConfig config,
            TelemetryLogger logger
        ) {
            if (string.IsNullOrWhiteSpace( config.ConfigurationFile ) == false) {
                logger.ConfigureFromLog4NetXmlConfigFile( new( config.ConfigurationFile ) );
            } else {
                if (config.EnableConsoleLog) {
                    ConfigureConsoleAppender(
                        logger,
                        config.ConsoleConfiguration
                    );
                }

                if (config.EnableDefaultLog) {
                    ConfigureDefaultLogAppender(
                        logger,
                        config.DefaultLogConfiguration
                    );
                }

                if (config.EnableTelemetryLog) {
                    ConfigureTelemetryAppender(
                        logger,
                        config.TelemetryLogConfiguration
                    );
                }
            }
        }

        private static void ConfigureConsoleAppender(
            TelemetryLogger logger,
            ConsoleLogConfig? consoleConfig
        ) {
            if (consoleConfig == null) {
                Console.Error.WriteLine(
                    "Failed to add Log4Net ConsoleAppender. Console logging will be limited." +
                    "Cannot Enable Console Log if ConsoleConfig Is Null."
                );
                return;
            }

            logger.AddConsoleAppender(
                consoleConfig.UseStdErr,
                consoleConfig.LogLevels,
                consoleConfig.EnableColoredConsole
            );
        }

        private static void ConfigureDefaultLogAppender(
            TelemetryLogger logger,
            DefaultLogConfig? logConfig
        ) {
            if (logConfig == null) {
                Console.Error.WriteLine(
                    "Failed to add default Log4Net RollingLogAppender. Logging may be limited.\n" +
                    "Cannot Enable Default Log if Log Config Is Null."
                );
                return;
            }
            logger.AddRollingLogAppender(
                Path.Join(
                    logConfig.LogDirectory,
                    logConfig.FileName
                ),
                logConfig.RolloverCount,
                logConfig.MaximumSize,
                logConfig.LogLevels
            );
        }

        private static void ConfigureTelemetryAppender(
            TelemetryLogger logger,
            TelemetryLogConfig? logConfig
        ) {
            if (logConfig == null) {
                Console.Error.WriteLine(
                    "Failed to add telemetry Log4Net RollingLogAppender.\n" +
                    "Cannot Enable Telemetry Log if TelemetryConfig Is Null."
                );
                return;
            }
            logger.AddTelemetryAppender(
                Path.Join(
                    logConfig.LogDirectory,
                    logConfig.FileName
                ),
                logConfig.RolloverCount,
                logConfig.MaximumSize
            );
        }
    }
}
