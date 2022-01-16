using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Logging.Types {
#nullable disable
    public class Log4NetProvider : ILoggerProvider {

        readonly TelemetryLogger _logger;

        private readonly ConcurrentDictionary<string, ILogger> _loggers = new( );

        public Log4NetProvider( TelemetryLogger logger ) { _logger = logger; }

        public ILogger CreateLogger( string categoryName ) {
            return _loggers.GetOrAdd( categoryName, CreateLoggerImplementation );
        }

        void IDisposable.Dispose( ) {
            GC.SuppressFinalize( this );
            _loggers.Clear( );
        }

        private ILogger CreateLoggerImplementation( string name ) {
            return _logger;
        }
    }
#nullable enable
}
