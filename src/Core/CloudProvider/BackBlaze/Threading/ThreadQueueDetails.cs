using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading {
    internal class ThreadQueueDetails {

        public readonly int ThreadCount;
        public readonly int PartSize;
        public readonly int FinalSize;
        public readonly int TotalParts;
        public readonly long FileSize;

        public ThreadQueueDetails(
            long fileSize,
            int recSize,
            int minPartSize,
            int threadCount,
            ILogger? log
        ) {
            //log;
            FileSize = fileSize;
            ThreadCount = threadCount;

            if ((threadCount * (long)recSize) > fileSize) {
                log?.LogDebug( "FileSize is smaller than the recommended part size multiplied by threads." );

                if ((fileSize / threadCount) > minPartSize) {
                    log?.LogDebug( "partSize is between recommended size and min size for $ThreadCount threads." );
                    int denumerator = (threadCount - 1 > 1) ? threadCount - 1 : 1;
                    PartSize = (int)Math.Floor( Convert.ToDecimal( fileSize / denumerator ) );
                    FinalSize = (int)(fileSize - (PartSize * denumerator));
                    TotalParts = threadCount;
                } else {
                    log?.LogDebug( "PartSize set to minSize. FinalSize is greater than partsize." );
                    PartSize = minPartSize;
                    TotalParts = (int)Math.Floor( Convert.ToDecimal( fileSize / minPartSize ) );
                    TotalParts = (TotalParts > 0) ? TotalParts : 1;
                    FinalSize = (int)(fileSize - (TotalParts * minPartSize));
                    if (FinalSize < minPartSize) {
                        // Handle remainder being smaller than minimum chunk size.
                        FinalSize += minPartSize;
                    }
                }
            } else {
                log?.LogDebug( "FileSize is greater than recommended part size multiplied by threads." );
                PartSize = recSize;
                TotalParts = (int)Math.Floor( Convert.ToDecimal( fileSize / recSize ) );
                FinalSize = (int)(fileSize - (TotalParts * (long)recSize));
                TotalParts++;
                if (FinalSize < minPartSize) {
                    // Handle remainder being smaller than minimum chunk size.
                    TotalParts--;
                    FinalSize += recSize;
                }
            }

            log?.LogDebug( "ThreadCount: {int}", ThreadCount );
            log?.LogDebug( "FileSize:    {long}", FileSize );
            log?.LogDebug( "recSize:     {int}", recSize );
            log?.LogDebug( "minPartSize: {int}", minPartSize );
            log?.LogDebug( "threadCount: {int}", threadCount );
            log?.LogDebug( "PartSize:    {int}", PartSize );
            log?.LogDebug( "FinalSize:   {int}", FinalSize );
            log?.LogDebug( "TotalParts:  {int}", TotalParts );

        }
    }
}

