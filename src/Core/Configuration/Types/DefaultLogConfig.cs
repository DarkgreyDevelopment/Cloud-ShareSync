using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Configuration.Enums;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// Configuration values for the built in rolling log file process.
    /// </summary>
    public class DefaultLogConfig {

        #region FileName

        /// <summary>
        /// <para>
        /// The file name and extension for the primary rolling log file.
        /// </para>
        /// If <see cref="RolloverCount"/> is greater than 0 then logs will roll over 
        /// and be renamed with the format "{FileName}.{LogNumber}.{Extension}" once the 
        /// <see cref="MaximumSize"/> has been met.
        /// </summary>
        /// <value>Cloud-ShareSync.log</value>
        public string FileName { get; set; } = "Cloud-ShareSync.log";

        private static Option<string> NewFileNameOption( Command verbCommand ) {
            Option<string> fileNameOption = new(
                name: "--FileName",
                description: "Specify the file name and extension for the primary rolling log file.",
                getDefaultValue: ( ) => "Cloud-ShareSync.log"
            );
            fileNameOption.AddAlias( "-n" );

            verbCommand.AddOption( fileNameOption );

            return fileNameOption;
        }

        #endregion FileName


        #region LogDirectory

        /// <summary>
        /// <para>
        /// The path, either relative or complete, to the directory
        /// where the rolling log files should be output.
        /// </para>
        /// Default value is the applications root directory.
        /// </summary>
        /// <value><code>Path.Join( <seealso cref="AppContext.BaseDirectory"/>, "log" )</code></value>
        public string LogDirectory { get; set; } = Path.Join( AppContext.BaseDirectory, "log" );

        private static Option<DirectoryInfo> NewLogDirectoryOption( Command verbCommand ) {
            Option<DirectoryInfo> logDirectoryOption = new(
                name: "--LogDirectory",
                description: "Specify the directory for the rolling log process to use.",
                getDefaultValue: ( ) => new( Path.Join( AppContext.BaseDirectory, "log" ) )
            );
            logDirectoryOption.AddAlias( "-d" );

            verbCommand.AddOption( logDirectoryOption );

            return logDirectoryOption;
        }

        #endregion LogDirectory



        #region RolloverCount

        /// <summary>
        /// The number of <see cref="MaximumSize"/> log files to keep.
        /// </summary>
        /// <value>5</value>
        public int RolloverCount { get; set; } = 5;

        private static Option<int> NewRolloverCountOption( Command verbCommand ) {
            Option<int> rolloverCountOption = new(
                name: "--RolloverCount",
                description: "Specify the number of MaximumSize log files to keep.",
                getDefaultValue: ( ) => 5
            );
            rolloverCountOption.AddAlias( "-r" );

            verbCommand.AddOption( rolloverCountOption );

            return rolloverCountOption;
        }

        #endregion RolloverCount



        #region MaximumSize

        /// <summary>
        /// The maximum size, in megabytes, that each log should get to before
        /// rolling over into a new file.
        /// </summary>
        /// <value>5</value>
        public int MaximumSize { get; set; } = 5;

        private static Option<int> NewMaximumSizeOption( Command verbCommand ) {
            Option<int> maximumSizeOption = new(
                name: "--MaximumSize",
                description:
                "Specify the maximum size, in megabytes, that each log should get to before rolling over into a new file.",
                getDefaultValue: ( ) => 5
            );
            maximumSizeOption.AddAlias( "-m" );

            verbCommand.AddOption( maximumSizeOption );

            return maximumSizeOption;
        }

        #endregion MaximumSize



        #region LogLevels

        /// <summary>
        /// Sets the log levels that should go into the rolling log file.
        /// </summary>
        /// <value>
        /// <see cref="SupportedLogLevels.Info"/><br/>
        /// <see cref="SupportedLogLevels.Warn"/><br/>
        /// <see cref="SupportedLogLevels.Error"/><br/>
        /// <see cref="SupportedLogLevels.Fatal"/><br/>
        /// </value>
        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public SupportedLogLevels LogLevels { get; set; } =
            SupportedLogLevels.Info |
            SupportedLogLevels.Warn |
            SupportedLogLevels.Error |
            SupportedLogLevels.Fatal; //30

        private static Option<SupportedLogLevels> NewLogLevelsOption( Command verbCommand ) {
            Option<SupportedLogLevels> logLevels = new(
                name: "--LogLevels",
                description: "Specify the log levels that should go into the rolling log file. " +
                             "Supported Log Levels: Fatal, Error, Warn, Info, Debug, Telemetry",
                getDefaultValue: ( ) => (SupportedLogLevels)30
            );
            logLevels.AddAlias( "-l" );

            verbCommand.AddOption( logLevels );

            return logLevels;
        }

        #endregion LogLevels


        #region VerbHandling

        public static Command NewRollingLogConfigCommand( Option<FileInfo> configPath ) {
            Command rollingLogConfig = new( "RollingLog" );
            rollingLogConfig.AddAlias( "rollinglog" );
            rollingLogConfig.Description = "Configure the Cloud-ShareSync default rolling log settings.";

            SetRollingLogConfigHandler(
                rollingLogConfig,
                NewFileNameOption( rollingLogConfig ),
                NewLogDirectoryOption( rollingLogConfig ),
                NewRolloverCountOption( rollingLogConfig ),
                NewMaximumSizeOption( rollingLogConfig ),
                NewLogLevelsOption( rollingLogConfig ),
                configPath
            );
            return rollingLogConfig;
        }

        internal static void SetRollingLogConfigHandler(
            Command rollingLogConfig,
            Option<string> fileName,
            Option<DirectoryInfo> logDirectory,
            Option<int> rolloverCount,
            Option<int> maximumSize,
            Option<SupportedLogLevels> logLevels,
            Option<FileInfo> configPath
        ) {
            rollingLogConfig.SetHandler( (
                    string fileName,
                    DirectoryInfo logDirectory,
                    int rolloverCount,
                    int maximumSize,
                    SupportedLogLevels logLevels,
                    FileInfo configPath
                 ) => {
                     if (configPath != null) { ConfigManager.SetAltDefaultConfigPath( configPath.FullName ); }

                     DefaultLogConfig config = new( ) {
                         FileName = fileName,
                         LogDirectory = logDirectory.FullName,
                         RolloverCount = rolloverCount,
                         MaximumSize = maximumSize,
                         LogLevels = logLevels
                     };
                     Console.WriteLine( $"{config}" );
                 },
                fileName,
                logDirectory,
                rolloverCount,
                maximumSize,
                logLevels,
                configPath
            );
        }

        #endregion VerbHandling

        /// <summary>
        /// Returns the <see cref="DefaultLogConfig"/> as a json string.
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
