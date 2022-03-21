using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading {
    internal class ThreadQueueDetails {

        public int _threadCount;
        public int _partSize;
        public int _finalSize;
        public int _totalParts;
        public long _fileSize;

        public ThreadQueueDetails( ) { }

        public ThreadQueueDetails(
            long fileSize,
            int recSize,
            int minPartSize,
            int threadCount,
            ILogger? log
        ) {
            if (fileSize <= 5242879) {
                throw new ArgumentException(
                    "FileSize must be greater than 5MiB (5242880 bytes).",
                    nameof( fileSize )
                );
            }
            _fileSize = fileSize;
            _threadCount = threadCount;

            AssignPartSizes( recSize, minPartSize );

            log?.LogDebug( "{string}", ToString( recSize, minPartSize ) );
        }

        public string ToString( int recSize, int minPartSize ) {
            return
            $"ThreadCount: {_threadCount}" +
            $"FileSize:    {_fileSize}" +
            $"TotalParts:  {_totalParts}" +
            $"PartSize:    {_partSize}" +
            $"FinalSize:   {_finalSize}" +
            $"recSize:     {recSize}" +
            $"minPartSize: {minPartSize}";
        }

        internal static int GetTotalParts( long fileSize, int denumerator ) {
            return (int)Math.Floor( Convert.ToDecimal( fileSize / denumerator ) );
        }

        internal void SetFinalSize( ) {
            // Add remainder onto final upload part.
            _finalSize = (int)(_fileSize - (_totalParts * _partSize)) + _partSize;
        }

        internal void AssignPartSizes(
            int recSize,
            int minPartSize
        ) {
            if ((_threadCount * (long)recSize) <= _fileSize) {
                MatchesRecSize( recSize );
            } else if ((_fileSize / _threadCount) > minPartSize) {
                BetweenRecSizeAndMinSize( );
            } else {
                MatchesMinSize( minPartSize );
            }
            SetFinalSize( );
        }

        internal void MatchesMinSize( int minPartSize ) {
            _partSize = minPartSize;
            _totalParts = GetTotalParts( _fileSize, _partSize );
            _totalParts = (_totalParts > 0) ? _totalParts : 1;
        }

        internal void BetweenRecSizeAndMinSize( ) {
            _totalParts = _threadCount;
            _partSize = GetTotalParts( _fileSize, _totalParts );
        }

        internal void MatchesRecSize( int recSize ) {
            _partSize = recSize;
            _totalParts = GetTotalParts( _fileSize, _partSize );
        }
    }
}

