using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    internal class ThreadArbiter {

        private readonly long _fileSize;
        private readonly int _recSize;
        private readonly int _minPartSize;
        public int ThreadCount { get; private set; }
        public int PartSize { get; private set; }
        public int FinalSize { get; private set; }
        public int TotalParts { get; private set; }

        public ThreadArbiter(
            long fileSize,
            int recSize,
            int minPartSize,
            int threadCount,
            ILogger? log
        ) {
            if (fileSize < 5242880) {
                throw new ArgumentException(
                    "FileSize must be greater than 5MiB (5242880 or more bytes).",
                    nameof( fileSize )
                );
            }
            _fileSize = fileSize;
            _recSize = recSize;
            _minPartSize = minPartSize;
            AssignPartSizes( threadCount );
            ThreadCount = threadCount > TotalParts ? TotalParts : threadCount;
            log?.LogDebug( "{string}", ToString( ) );
        }

        public override string ToString( ) {
            return "\n" +
            $"ThreadCount: {ThreadCount}\n" +
            $"FileSize:    {_fileSize}\n" +
            $"TotalParts:  {TotalParts}\n" +
            $"PartSize:    {PartSize}\n" +
            $"FinalSize:   {FinalSize}\n" +
            $"recSize:     {_recSize}\n" +
            $"minPartSize: {_minPartSize}";
        }

        private void AssignPartSizes( int threadCount ) {
            if ((threadCount * (long)_recSize) <= _fileSize) {
                MatchesRecSize( );
            } else if ((_fileSize / threadCount) > _minPartSize) {
                BetweenRecSizeAndMinSize( threadCount );
            } else {
                MatchesMinSize( );
            }
            SetFinalSize( );
        }

        private void MatchesMinSize( ) {
            PartSize = _minPartSize;
            TotalParts = GetTotalParts( _fileSize, PartSize );
            TotalParts = (TotalParts > 0) ? TotalParts : 1;
        }

        private void BetweenRecSizeAndMinSize( int threadCount ) {
            TotalParts = threadCount;
            PartSize = GetTotalParts( _fileSize, TotalParts );
        }

        private void MatchesRecSize( ) {
            PartSize = _recSize;
            TotalParts = GetTotalParts( _fileSize, PartSize );
        }

        private static int GetTotalParts( long fileSize, int denumerator ) =>
            (int)Math.Floor( Convert.ToDecimal( fileSize / denumerator ) );

        private void SetFinalSize( ) {
            // Add remainder onto final upload part.
            FinalSize = (int)(_fileSize - (TotalParts * PartSize)) + PartSize;
        }

    }
}
