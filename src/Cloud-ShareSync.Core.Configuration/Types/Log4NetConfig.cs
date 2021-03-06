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

        /// <summary>
        /// To override the default logging configuration specify the path to a log4net (xml) config file.
        /// </summary>
        public string? ConfigurationFile { get; set; }


        /// <summary>
        /// By default Cloud-ShareSync implements a custom rolling log file process if <see cref="ConfigurationFile"/> 
        /// is not set.<br/>
        /// This field enables or disables the built in rolling log file configuration.
        /// </summary>
        public bool EnableDefaultLog { get; set; } = true;


        /// <summary>
        /// The configuration settings for the built in rolling log process.<br/>
        /// This field is required if <see cref="EnableDefaultLog"/> is true.
        /// </summary>
        public DefaultLogConfig? DefaultLogConfiguration { get; set; } = new DefaultLogConfig( );

        /// <summary>
        /// Cloud-ShareSync implements a custom rolling log file process for
        /// OpenTelemetry content if <see cref="ConfigurationFile"/> is not set.<br/>
        /// This field enables or disables the built in telemetry log configuration.
        /// </summary>
        public bool EnableTelemetryLog { get; set; }


        /// <summary>
        /// The configuration settings for the built in OpenTelemetry log export process. <br/>
        /// This field is required if <see cref="EnableTelemetryLog"/> is true.
        /// </summary>
        public TelemetryLogConfig? TelemetryLogConfiguration { get; set; } = new TelemetryLogConfig( );


        /// <summary>
        /// Cloud-ShareSync implements a colored console appender if <see cref="ConfigurationFile"/>
        /// is not set.<br/>
        /// This field enables or disables the built in colored console configuration.
        /// </summary>
        public bool EnableConsoleLog { get; set; } = true;


        /// <summary>
        /// The configuration settings for the built in console log process.<br/>
        /// This field is required if <see cref="EnableConsoleLog"/> is true.
        /// </summary>
        public ConsoleLogConfig? ConsoleConfiguration { get; set; } = new ConsoleLogConfig( );


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
