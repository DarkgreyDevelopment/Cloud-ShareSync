using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Configuration.Enums;

namespace Cloud_ShareSync.Core.Configuration.Types.Logging {
#nullable disable
    /// <summary>
    /// Configuration values for the built in console log process.
    /// </summary>
    public class ConsoleLogConfig {

        /// <summary>
        /// Sets the log levels that should go into the console.
        /// </summary>
        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public SupportedLogLevels LogLevels { get; set; } =
            SupportedLogLevels.Info |
            SupportedLogLevels.Warn |
            SupportedLogLevels.Error |
            SupportedLogLevels.Fatal;

        /// <summary>
        /// Controls whether Fatal and Error messages are written to the stderr stream.
        /// If set to false all console messages, regardless of severity, will go to stdout instead.
        /// </summary>
        public bool UseStdErr { get; set; } = true;

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
