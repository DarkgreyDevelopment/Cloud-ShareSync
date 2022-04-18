using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Configuration.Interfaces;
using Cloud_ShareSync.Core.Logging;

namespace Cloud_ShareSync.Configuration.Types {
#nullable disable
    /// <summary>
    /// Configuration values for the built in rolling log file process.
    /// </summary>
    public class DefaultLogConfig : ICloudShareSyncConfig {

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


        /// <summary>
        /// <para>
        /// The path, either relative or complete, to the directory
        /// where the rolling log files should be output.
        /// </para>
        /// Default value is the applications root directory.
        /// </summary>
        /// <value><code>Path.Join( <seealso cref="AppContext.BaseDirectory"/>, "log" )</code></value>
        public string LogDirectory { get; set; } = Path.Join( AppContext.BaseDirectory, "log" );


        /// <summary>
        /// The number of <see cref="MaximumSize"/> log files to keep.
        /// </summary>
        /// <value>5</value>
        public int RolloverCount { get; set; } = 5;


        /// <summary>
        /// The maximum size, in megabytes, that each log should get to before
        /// rolling over into a new file.
        /// </summary>
        /// <value>5</value>
        public int MaximumSize { get; set; } = 5;


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
