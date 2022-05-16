using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Configuration.Interfaces;
using Cloud_ShareSync.Core.Logging;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// Configuration values for the built in console log process.
    /// </summary>
    public class ConsoleLogConfig : ICloudShareSyncConfig {

        /// <summary>
        /// Controls whether Fatal and Error messages are written to the stderr stream.<br/>
        /// If set to false all console messages, regardless of severity, will go to stdout instead.
        /// </summary>
        /// <value><see langword="true"/></value>
        public bool UseStdErr { get; set; } = true;


        /// <summary>
        /// Controls whether to use the colored console appender or plaintext console appender.<br/>
        /// Default settings enable color unless <a href="https://no-color.org">Env:NO_COLOR</a> is set.<br/>
        /// Set this value to false to explicitly disable the colored console appender.
        /// </summary>
        /// <value><see langword="true"/></value>
        public bool EnableColoredConsole { get; set; } = true;


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
