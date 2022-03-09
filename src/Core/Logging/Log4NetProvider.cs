using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Logging {
#nullable disable
    internal class Log4NetProvider : ILoggerProvider {

        readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, ILogger> _loggers = new( );

        public Log4NetProvider( ILogger logger ) {
            _logger = logger;
        }

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
