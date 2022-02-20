using System.Collections.Concurrent;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {
        internal async Task<UploadB2File> NewLargeFileUpload( UploadB2File uploadObject ) {

            int recSize = RecommendedPartSize ?? 0;
            int totalParts = (int)Math.Floor( Convert.ToDecimal( uploadObject.FilePath.Length / recSize ) );
            int finalLength = (int)(uploadObject.FilePath.Length - (totalParts * (long)recSize));
            totalParts++;

            if (finalLength < (AbsoluteMinimumPartSize ?? 0)) {
                // Handle Edge Cases where remainder is smaller than minimum chunk size.
                totalParts--;
                finalLength += recSize;
            }

            int threadCount = B2ThreadManager.GetActiveThreadCount( totalParts );
            B2ConcurrentStats concurrencyStats = new( threadCount );
            _log?.LogInformation( "Uploading Large File Parts Async" );
            _log?.LogInformation( "Splitting file into {int} {int} byte chunks and 1 {int} chunk.", totalParts - 1, recSize, finalLength );
            _log?.LogInformation( "Chunks will be uploaded asyncronously via {int} upload streams.", threadCount );

            ConcurrentBag<LargeFilePartReturn>? resultsList = new( );
            ConcurrentStack<FilePartInfo> filePartQueue = new( );
            long lengthTotal = 0;
            // Populate the queue.
            for (int i = 1; i <= totalParts; i++) {
                int partLength = i == totalParts ? finalLength : recSize;
                filePartQueue.Push( new FilePartInfo( i, partLength ) );
                lengthTotal += partLength;
            }
            if (lengthTotal != uploadObject.FilePath.Length) {
                _log?.LogCritical( $"filePartQueue part length total does not equal files length." );
                throw new InvalidOperationException( "Failed to upload full file." );
            }

            List<Task<bool>> uploadTasks = new( );

            for (int thread = 0; thread < threadCount; thread++) {
                _log?.LogDebug( "Thread#{string} - Adding Task to Task List.", thread );
                uploadTasks.Add(
                    UploadLargeFileParts(
                        uploadObject,
                        recSize,
                        resultsList,
                        filePartQueue,
                        thread,
                        concurrencyStats
                    )
                );
                Thread.Sleep( 100 );
            }

            while (uploadTasks.Any( x => x.IsCompleted == false )) { Thread.Sleep( 1000 ); }
            DetermineMultiPartUploadSuccessStatus( uploadTasks, filePartQueue );
            _log?.LogInformation( "Uploaded Large File Parts Async" );

            _log?.LogInformation( "Finishing Large File Upload." );
            if (resultsList != null) {
                foreach (LargeFilePartReturn result in resultsList) {
                    uploadObject.Sha1PartsList.Add( new( result.PartNumber, result.Sha1Hash ) );
                    uploadObject.TotalBytesSent += result.DataSize;
                }
            }

            _log?.LogInformation( "Concurrency Stats: {string}", concurrencyStats );
            B2ThreadManager.ConcurrencyStats.Add( concurrencyStats );

            _log?.LogInformation( "Thread UploadStats:" );
            B2ThreadManager.ShowThreadStatistics( true );


            await FinishUploadLargeFile( uploadObject );

            foreach (FailureInfo failure in B2ThreadManager.FailureDetails) {
                failure.Reset( );
            }

            return uploadObject;
        }

    }
}
