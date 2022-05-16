using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Interfaces;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// Configuration values for the built in rolling telemetry log file process.
    /// </summary>
    public class TelemetryLogConfig : ICloudShareSyncConfig {

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


        /// <summary>
        /// The path, either relative or complete, to the directory
        /// where the rolling log files should be output.
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
