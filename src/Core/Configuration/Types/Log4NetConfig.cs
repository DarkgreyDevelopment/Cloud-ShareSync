using System.CommandLine;
using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Interfaces;
using Cloud_ShareSync.Core.Logging;

namespace Cloud_ShareSync.Core.Configuration.Types {
    /// <summary>
    /// <para>
    /// Cloud-ShareSync instruments logging via a custom ILogger implementation (ref <see cref="TelemetryLogger"/>)
    /// that utilizes apache Log4Net for its backend.
    /// </para>
    /// <para>
    /// The built in/default Log4Net configuration consists of a rolling log file appender
    /// and colored console appender for all standard log messages.<br/>
    /// OpenTelemetry traces are also exported, in json format, to a rolling log file appender.
    /// </para>
    /// These built in settings can also optionally be overridden via a log4net XML configuration file.
    /// </summary>
    public class Log4NetConfig : ICloudShareSyncConfig {

        public Log4NetConfig( bool defaultsEnabled ) {
            if (defaultsEnabled == false) {
                ConfigurationFile = null;
                EnableDefaultLog = false;
                DefaultLogConfiguration = null;
                EnableTelemetryLog = false;
                TelemetryLogConfiguration = null;
                EnableConsoleLog = false;
            }
        }

        public Log4NetConfig( ) { }

        #region ConfigurationFile

        /// <summary>
        /// To override the default logging configuration specify the path to a log4net (xml) config file.
        /// </summary>
        public string? ConfigurationFile { get; set; }

        private static Option<string> NewConfigurationFileOption( Command verbCommand ) {
            Option<string> configurationFileOption = new(
                name: "--ConfigurationFile",
                description: "Specify the path to a log4net (xml) config file.",
                getDefaultValue: ( ) => string.Empty
            );

            verbCommand.AddOption( configurationFileOption );

            return configurationFileOption;
        }

        #endregion ConfigurationFile


        #region EnableDefaultLog

        /// <summary>
        /// By default Cloud-ShareSync implements a custom rolling log file process if <see cref="ConfigurationFile"/> 
        /// is not set.<br/>
        /// This field enables or disables the built in rolling log file configuration.
        /// </summary>
        public bool EnableDefaultLog { get; set; } = true;

        private static Option<bool> NewEnableDefaultLogOption( Command verbCommand ) {
            Option<bool> enableDefaultLogOption = new(
                name: "--EnableDefaultLog",
                description: "Enable/disable the default rolling log file process.",
                getDefaultValue: ( ) => true
            );
            enableDefaultLogOption.AddAlias( "-d" );

            verbCommand.AddOption( enableDefaultLogOption );

            return enableDefaultLogOption;
        }

        #endregion EnableDefaultLog


        #region DefaultLogConfiguration

        /// <summary>
        /// The configuration settings for the built in rolling log process.<br/>
        /// This field is required if <see cref="EnableDefaultLog"/> is true.
        /// </summary>
        public DefaultLogConfig? DefaultLogConfiguration { get; set; } = new DefaultLogConfig( );

        #endregion DefaultLogConfiguration


        #region EnableTelemetryLog

        /// <summary>
        /// Cloud-ShareSync implements a custom rolling log file process for
        /// OpenTelemetry content if <see cref="ConfigurationFile"/> is not set.<br/>
        /// This field enables or disables the built in telemetry log configuration.
        /// </summary>
        public bool EnableTelemetryLog { get; set; }

        private static Option<bool> NewEnableTelemetryLogOption( Command verbCommand ) {
            Option<bool> enableTelemetryLogOption = new(
                name: "--EnableTelemetryLog",
                description: "Enable/disable the telemetry log file process.",
                getDefaultValue: ( ) => false
            );
            enableTelemetryLogOption.AddAlias( "-t" );

            verbCommand.AddOption( enableTelemetryLogOption );

            return enableTelemetryLogOption;
        }

        #endregion EnableTelemetryLog


        #region TelemetryLogConfiguration

        /// <summary>
        /// The configuration settings for the built in OpenTelemetry log export process. <br/>
        /// This field is required if <see cref="EnableTelemetryLog"/> is true.
        /// </summary>
        public TelemetryLogConfig? TelemetryLogConfiguration { get; set; } = new TelemetryLogConfig( );

        #endregion TelemetryLogConfiguration


        #region EnableConsoleLog

        /// <summary>
        /// Cloud-ShareSync implements a colored console appender if <see cref="ConfigurationFile"/>
        /// is not set.<br/>
        /// This field enables or disables the built in colored console configuration.
        /// </summary>
        public bool EnableConsoleLog { get; set; } = true;

        private static Option<bool> NewEnableConsoleLogOption( Command verbCommand ) {
            Option<bool> enableConsoleLogOption = new(
                name: "--EnableConsoleLog",
                description: "Enable/disable the console log process.",
                getDefaultValue: ( ) => true
            );
            enableConsoleLogOption.AddAlias( "-c" );

            verbCommand.AddOption( enableConsoleLogOption );

            return enableConsoleLogOption;
        }

        #endregion EnableConsoleLog


        #region ConsoleConfiguration

        /// <summary>
        /// The configuration settings for the built in console log process.<br/>
        /// This field is required if <see cref="EnableConsoleLog"/> is true.
        /// </summary>
        public ConsoleLogConfig? ConsoleConfiguration { get; set; } = new ConsoleLogConfig( );

        #endregion ConsoleConfiguration


        #region VerbHandling

        public static Command NewLoggingConfigCommand( Option<FileInfo> configPath ) {
            Command loggingConfig = new( "Logging" );
            loggingConfig.AddAlias( "logging" );
            loggingConfig.AddAlias( "log" );
            loggingConfig.Description = "Configure the Cloud-ShareSync logging settings.";

            SetLoggingConfigHandler(
                loggingConfig,
                NewConfigurationFileOption( loggingConfig ),
                NewEnableDefaultLogOption( loggingConfig ),
                NewEnableTelemetryLogOption( loggingConfig ),
                NewEnableConsoleLogOption( loggingConfig ),
                configPath
            );
            return loggingConfig;
        }

        internal static void SetLoggingConfigHandler(
            Command databaseConfig,
            Option<string> configurationFile,
            Option<bool> enableDefaultLog,
            Option<bool> enableTelemetryLog,
            Option<bool> enableConsoleLog,
            Option<FileInfo> configPath
        ) {
            databaseConfig.SetHandler( (
                 string configurationFile,
                 bool enableDefaultLog,
                 bool enableTelemetryLog,
                 bool enableConsoleLog,
                 FileInfo configPath
                 ) => {
                     if (configPath != null) { ConfigManager.SetAltDefaultConfigPath( configPath.FullName ); }

                     Log4NetConfig config = new( ) {
                         ConfigurationFile = configurationFile,
                         EnableDefaultLog = enableDefaultLog,
                         EnableTelemetryLog = enableTelemetryLog,
                         EnableConsoleLog = enableConsoleLog
                     };
                     new ConfigManager( ).UpdateConfigSection( config );
                 },
                configurationFile,
                enableDefaultLog,
                enableTelemetryLog,
                enableConsoleLog,
                configPath
            );
        }

        #endregion VerbHandling


        /// <summary>
        /// Returns the <see cref="Log4NetConfig"/> as a json string.
        /// </summary>
        public override string ToString( ) =>
            JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
    }
}
