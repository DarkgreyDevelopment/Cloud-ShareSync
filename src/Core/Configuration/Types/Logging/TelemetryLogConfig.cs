using System.Reflection;
using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types.Logging {
#nullable disable
    /// <summary>
    /// Configuration values for the built in rolling telemetry log file process.
    /// </summary>
    public class TelemetryLogConfig {
        private static readonly string s_assemblyPath = Directory.GetParent(
                                        Assembly.GetExecutingAssembly( ).Location
                                     )?.FullName ?? "";
        /// <summary>
        /// <para>
        /// The file name and extension for the telemetry rolling log file.
        /// </para>
        /// <para>
        /// If (<see cref="RolloverCount"/> > 0) then logs will roll over 
        /// and be renamed with the format "FileName.#.Extension" once the 
        /// <see cref="MaximumSize"/> has been met.
        /// </para>
        /// Default value set to "Cloud-ShareSync-Telemetry.log".
        /// </summary>
        public string FileName { get; set; } = "Cloud-ShareSync-Telemetry.log";

        /// <summary>
        /// <para>
        /// The path, either relative or complete, to the directory
        /// where the rolling log files should be output.
        /// </para>
        /// Default value is the applications root directory.
        /// </summary>
        public string LogDirectory { get; set; } = s_assemblyPath;

        /// <summary>
        /// <para>
        /// The number of <see cref="MaximumSize"/> log files to keep.
        /// </para>
        /// The default value is 50.
        /// </summary>
        public int RolloverCount { get; set; } = 50;

        /// <summary>
        /// <para>
        /// The maximum size, in megabytes, that each log should get to before
        /// rolling over into a new file.
        /// </para>
        /// The default value is 5.
        /// </summary>
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
