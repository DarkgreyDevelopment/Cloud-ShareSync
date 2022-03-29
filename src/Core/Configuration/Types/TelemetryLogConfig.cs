using System.CommandLine;
using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// Configuration values for the built in rolling telemetry log file process.
    /// </summary>
    public class TelemetryLogConfig {

        #region FileName

        /// <summary>
        /// <para>
        /// The file name and extension for the telemetry rolling log file.
        /// </para>
        /// If <see cref="RolloverCount"/> is greater than 0 then logs will roll over 
        /// and be renamed with the format "{FileName}.{LogNumber}.{Extension}" once the 
        /// <see cref="MaximumSize"/> has been met.
        /// </summary>
        /// <value>Cloud-ShareSync-Telemetry.log</value>
        public string FileName { get; set; } = "Cloud-ShareSync-Telemetry.log";

        private static Option<string> NewFileNameOption( Command verbCommand ) {
            Option<string> fileNameOption = new(
                name: "--FileName",
                description: "Specify the file name and extension for the primary telemetry log file.",
                getDefaultValue: ( ) => "Cloud-ShareSync-Telemetry.log"
            );
            fileNameOption.AddAlias( "-n" );

            verbCommand.AddOption( fileNameOption );

            return fileNameOption;
        }

        #endregion FileName


        #region LogDirectory

        /// <summary>
        /// The path, either relative or complete, to the directory
        /// where the rolling log files should be output.
        /// </summary>
        /// <value><code>Path.Join( <seealso cref="AppContext.BaseDirectory"/>, "log" )</code></value>
        public string LogDirectory { get; set; } = Path.Join( AppContext.BaseDirectory, "log" );

        private static Option<DirectoryInfo> NewLogDirectoryOption( Command verbCommand ) {
            Option<DirectoryInfo> logDirectoryOption = new(
                name: "--LogDirectory",
                description: "Specify the directory for the telemetry log process to use.",
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
                description: "Specify the number of MaximumSize telemetry log files to keep.",
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


        #region VerbHandling

        public static Command NewTelemetryLogConfigCommand( Option<FileInfo> configPath ) {
            Command telemetryLogConfig = new( "TelemetryLog" );
            telemetryLogConfig.AddAlias( "telemetrylog" );
            telemetryLogConfig.Description = "Configure the Cloud-ShareSync default rolling log settings.";

            SetTelemetryLogConfigHandler(
                telemetryLogConfig,
                NewFileNameOption( telemetryLogConfig ),
                NewLogDirectoryOption( telemetryLogConfig ),
                NewRolloverCountOption( telemetryLogConfig ),
                NewMaximumSizeOption( telemetryLogConfig ),
                configPath
            );
            return telemetryLogConfig;
        }

        internal static void SetTelemetryLogConfigHandler(
            Command telemetryLogConfig,
            Option<string> fileName,
            Option<DirectoryInfo> logDirectory,
            Option<int> rolloverCount,
            Option<int> maximumSize,
            Option<FileInfo> configPath
        ) {
            telemetryLogConfig.SetHandler( (
                     string fileName,
                     DirectoryInfo logDirectory,
                     int rolloverCount,
                     int maximumSize,
                     FileInfo configPath
                 ) => {
                     if (configPath != null) { ConfigManager.SetAltDefaultConfigPath( configPath.FullName ); }

                     TelemetryLogConfig config = new( ) {
                         FileName = fileName,
                         LogDirectory = logDirectory.FullName,
                         RolloverCount = rolloverCount,
                         MaximumSize = maximumSize

                     };
                     Console.WriteLine( $"{config}" );
                 },
                fileName,
                logDirectory,
                rolloverCount,
                maximumSize,
                configPath
            );
        }

        #endregion VerbHandling


        /// <summary>
        /// Returns the <see cref="TelemetryLogConfig"/> as a json string.
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
