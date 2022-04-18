namespace Cloud_ShareSync.Core.Logging {
    /// <summary>
    /// The enumerated log levels supported by Cloud_ShareSync.Core.Logging
    /// </summary>
    [Flags]
    public enum SupportedLogLevels {
        /// <summary>
        /// Fatal messages are sent when the application has encountered an unrecoverable error.
        /// </summary>
        Fatal = 2,

        /// <summary>
        /// Error messages are sent when the application has encountered a recoverable error.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Warning messages are sent when the application has encountered an unexpected situation.
        /// </summary>
        Warn = 8,

        /// <summary>
        /// Information messages are sent to report on the normal state of operation.
        /// </summary>
        Info = 16,

        /// <summary>
        /// Debug messages contain trace level detail and should not be enabled in production.
        /// Enabling debug logs in production could expose encrytion keys and passwords to the logs.
        /// </summary>
        Debug = 32,

        /// <summary>
        /// The Telemetry log level receives opentelemetry spans that contain the highest level of detail
        /// about the application state. OpenTelemetry data is exported to this stream in the format of json strings.
        /// </summary>
        Telemetry = 64
    }
}
