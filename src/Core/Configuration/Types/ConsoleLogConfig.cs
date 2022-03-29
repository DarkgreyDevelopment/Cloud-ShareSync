using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Configuration.Enums;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// Configuration values for the built in console log process.
    /// </summary>
    public class ConsoleLogConfig {

        #region UseStdErr

        /// <summary>
        /// Controls whether Fatal and Error messages are written to the stderr stream.<br/>
        /// If set to false all console messages, regardless of severity, will go to stdout instead.
        /// </summary>
        /// <value><see langword="true"/></value>
        public bool UseStdErr { get; set; } = true;

        private static Option<bool> NewUseStdErrOption( Command verbCommand ) {
            Option<bool> useStdErrOption = new(
                name: "--UseStdErr",
                description:
                    "Enable to send error messages to the stderr stream. Disable to send all output to the stdout stream.",
                getDefaultValue: ( ) => true
            );
            useStdErrOption.AddAlias( "-e" );

            verbCommand.AddOption( useStdErrOption );

            return useStdErrOption;
        }

        #endregion UseStdErr


        #region EnableColoredConsole

        /// <summary>
        /// Controls whether to use the colored console appender or plaintext console appender.<br/>
        /// Default settings enable color unless <a href="https://no-color.org">Env:NO_COLOR</a> is set.<br/>
        /// Set this value to false to explicitly disable the colored console appender.
        /// </summary>
        /// <value><see langword="true"/></value>
        public bool EnableColoredConsole { get; set; } = true;

        private static Option<bool> NewEnableColoredConsoleOption( Command verbCommand ) {
            Option<bool> enableColoredConsoleOption = new(
                name: "--EnableColoredConsole",
                description:
                "Enable to use colored console messages. Disable or set $Env:NO_COLOR to true to disable colored messages.",
                getDefaultValue: ( ) => true
            );
            enableColoredConsoleOption.AddAlias( "-c" );

            verbCommand.AddOption( enableColoredConsoleOption );

            return enableColoredConsoleOption;
        }

        #endregion EnableColoredConsole


        #region LogLevels

        /// <summary>
        /// Sets the log levels that should go into the console.
        /// </summary>
        /// <value>
        /// <see cref="SupportedLogLevels.Info"/><br/>
        /// <see cref="SupportedLogLevels.Warn"/><br/>
        /// <see cref="SupportedLogLevels.Error"/><br/>
        /// <see cref="SupportedLogLevels.Fatal"/><br/>
        /// </value>
        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public SupportedLogLevels LogLevels { get; set; } = //30
            SupportedLogLevels.Info |
            SupportedLogLevels.Warn |
            SupportedLogLevels.Error |
            SupportedLogLevels.Fatal;

        private static Option<SupportedLogLevels> NewLogLevelsOption( Command verbCommand ) {
            Option<SupportedLogLevels> logLevels = new(
                name: "--LogLevels",
                description: "Specify the log levels that should go into the console. " +
                             "Supported Log Levels: Fatal, Error, Warn, Info, Debug, Telemetry",
                getDefaultValue: ( ) => (SupportedLogLevels)30
            );
            logLevels.AddAlias( "-l" );

            verbCommand.AddOption( logLevels );

            return logLevels;
        }

        #endregion LogLevels


        #region VerbHandling

        public static Command NewConsoleLogConfigCommand( Option<FileInfo> configPath ) {
            Command consoleLogConfig = new( "ConsoleLog" );
            consoleLogConfig.AddAlias( "consolelog" );
            consoleLogConfig.Description = "Configure the Cloud-ShareSync console log settings.";

            SetConsoleLogConfigHandler(
                consoleLogConfig,
                NewUseStdErrOption( consoleLogConfig ),
                NewEnableColoredConsoleOption( consoleLogConfig ),
                NewLogLevelsOption( consoleLogConfig ),
                configPath
            );
            return consoleLogConfig;
        }

        internal static void SetConsoleLogConfigHandler(
            Command consoleLogConfig,
            Option<bool> useStdErr,
            Option<bool> enableColoredConsole,
            Option<SupportedLogLevels> logLevels,
            Option<FileInfo> configPath
        ) {
            consoleLogConfig.SetHandler( (
                     bool useStdErr,
                     bool enableColoredConsole,
                     SupportedLogLevels logLevels,
                     FileInfo configPath
                 ) => {
                     if (configPath != null) { ConfigManager.SetAltDefaultConfigPath( configPath.FullName ); }

                     ConsoleLogConfig config = new( ) {
                         UseStdErr = useStdErr,
                         EnableColoredConsole = enableColoredConsole,
                         LogLevels = logLevels
                     };
                     Console.WriteLine( $"{config}" );
                 },
                useStdErr,
                enableColoredConsole,
                logLevels,
                configPath
            );
        }

        #endregion VerbHandling


        /// <summary>
        /// Returns the <see cref="ConsoleLogConfig"/> as a json string.
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
#nullable enable
}
