using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Configuration.Enums;

namespace Cloud_ShareSync.Core.Configuration.Types.Logging {
#nullable disable
    /// <summary>
    /// Configuration values for the built in rolling log file process.
    /// </summary>
    public class DefaultLogConfig {
        /// <summary>
        /// <para>
        /// The file name and extension for the primary rolling log file.
        /// </para>
        /// <para>
        /// If (<see cref="RolloverCount"/> > 0) then logs will roll over 
        /// and be renamed with the format "FileName.#.Extension" once the 
        /// <see cref="MaximumSize"/> has been met.
        /// </para>
        /// Default value set to "Cloud-ShareSync.log".
        /// </summary>
        public string FileName { get; set; } = "Cloud-ShareSync.log";

        /// <summary>
        /// <para>
        /// The path, either relative or complete, to the directory
        /// where the rolling log files should be output.
        /// </para>
        /// Default value is the applications root directory.
        /// </summary>
        public string LogDirectory { get; set; } = Path.Join( AppContext.BaseDirectory, "log" );

        /// <summary>
        /// <para>
        /// The number of <see cref="MaximumSize"/> log files to keep.
        /// </para>
        /// The default value is 5.
        /// </summary>
        public int RolloverCount { get; set; } = 5;

        /// <summary>
        /// <para>
        /// The maximum size, in megabytes, that each log should get to before
        /// rolling over into a new file.
        /// </para>
        /// The default value is 5.
        /// </summary>
        public int MaximumSize { get; set; } = 5;

        /// <summary>
        /// Sets the log levels that should go into the rolling log file.
        /// </summary>
        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public SupportedLogLevels LogLevels { get; set; } =
            SupportedLogLevels.Info |
            SupportedLogLevels.Warn |
            SupportedLogLevels.Error |
            SupportedLogLevels.Fatal;

        /// <summary>
        /// Returns the <see cref="DefaultLogConfig"/> as a json string.
        /// </summary>
        /// <returns></returns>
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
