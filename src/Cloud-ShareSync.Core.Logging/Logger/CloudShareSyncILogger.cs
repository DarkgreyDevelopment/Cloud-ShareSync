using log4net;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Logging.Logger {
    public abstract class CloudShareSyncILogger : ILogger {

        public CloudShareSyncILogger( string serviceName ) {
            Log4NetLog = LogManager.GetLogger( serviceName );
        }

        internal readonly ILog? Log4NetLog;

        public IDisposable BeginScope<TState>( TState state ) => default!;

        public bool IsEnabled( LogLevel logLevel ) {
            return logLevel switch {
                LogLevel.Critical => IsFatalEnabled( ),
                LogLevel.Error => IsErrorEnabled( ),
                LogLevel.Warning => IsWarnEnabled( ),
                LogLevel.Information => IsInfoEnabled( ),
                LogLevel.Debug => IsDebugEnabled( ),
                LogLevel.Trace => IsDebugEnabled( ),
                LogLevel.None => false,
                _ => throw new ArgumentOutOfRangeException( nameof( logLevel ) )
            };
        }

        private bool IsFatalEnabled( ) { return Log4NetLog?.IsFatalEnabled ?? false; }
        private bool IsErrorEnabled( ) { return Log4NetLog?.IsErrorEnabled ?? false; }
        private bool IsWarnEnabled( ) { return Log4NetLog?.IsWarnEnabled ?? false; }
        private bool IsInfoEnabled( ) { return Log4NetLog?.IsInfoEnabled ?? false; }
        private bool IsDebugEnabled( ) { return Log4NetLog?.IsDebugEnabled ?? false; }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) {
            if (Log4NetLog == null || IsEnabled( logLevel ) == false) { return; }

            string message = $"{formatter( state, exception )} {exception}";

            if (string.IsNullOrEmpty( message ) == false) {
                WriteFatalMessage( Log4NetLog, logLevel, message );
                WriteErrorMessage( Log4NetLog, logLevel, message );
                WriteWarnMessage( Log4NetLog, logLevel, message );
                WriteInfoMessage( Log4NetLog, logLevel, message );
                WriteDebugMessage( Log4NetLog, logLevel, message );
            }
        }

        private static void WriteFatalMessage( ILog log, LogLevel level, string message ) {
            if (level == LogLevel.Critical) { log.Fatal( message ); }
        }
        private static void WriteErrorMessage( ILog log, LogLevel level, string message ) {
            if (level == LogLevel.Error) { log.Error( message ); }
        }
        private static void WriteWarnMessage( ILog log, LogLevel level, string message ) {
            if (level == LogLevel.Warning) { log.Warn( message ); }
        }
        private static void WriteInfoMessage( ILog log, LogLevel level, string message ) {
            if (level == LogLevel.Information) { log.Info( message ); }
        }
        private static void WriteDebugMessage( ILog log, LogLevel level, string message ) {
            if (level == LogLevel.Trace || level == LogLevel.Debug) { log.Debug( message ); }
        }
    }
}
