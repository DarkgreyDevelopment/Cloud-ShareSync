using System.Collections.Concurrent;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {
        internal async Task<UploadB2File> NewLargeFileUpload( UploadB2File uploadObject ) {

            int threadCount = B2ThreadManager.GetActiveThreadCount( );
            ThreadQueueDetails threadDeets = new(
                uploadObject.FilePath.Length,
                RecommendedPartSize ?? 0,
                AbsoluteMinimumPartSize ?? 0,
                threadCount,
                _log
            );

            ConcurrentBag<LargeFilePartReturn>? resultsList = new( );
            ConcurrentStack<FilePartInfo> filePartQueue = new( );
            long lengthTotal = 0;
            // Populate the queue.
            for (int i = 1; i <= threadDeets._totalParts; i++) {
                int partLength = i == threadDeets._totalParts ? threadDeets._finalSize : threadDeets._partSize;
                filePartQueue.Push( new FilePartInfo( i, partLength ) );
                lengthTotal += partLength;
            }
            if (lengthTotal != threadDeets._fileSize) {
                _log?.LogCritical( $"filePartQueue part length total does not equal files length." );
                throw new ApplicationException( "Failed to upload full file." );
            }

            B2ConcurrentStats concurrencyStats = new( threadCount, _log );
            B2ProcessStats uploadStats = new( uploadObject.FilePath.Length, _log );
            _log?.LogInformation( "Uploading Large File Parts Async" );
            _log?.LogInformation(
                "Splitting file into {int} {int} byte chunks and 1 {int} chunk.",
                threadDeets._totalParts - 1,
                threadDeets._partSize,
                threadDeets._finalSize
            );
            int taskTotal = threadCount > threadDeets._totalParts ? threadDeets._totalParts : threadCount;
            _log?.LogInformation( "Chunks will be uploaded asyncronously via {int} upload streams.", taskTotal );

            List<Task<bool>> uploadTasks = new( );
            for (int thread = 0; thread < taskTotal; thread++) {
                _log?.LogDebug( "Thread#{string} - Adding Task to Task List.", thread );
                uploadTasks.Add(
                    UploadLargeFileParts(
                        uploadObject,
                        threadDeets._partSize,
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
            await FinishUploadLargeFile( uploadObject );

            uploadStats.SetStopTime( );
            _log?.LogInformation( "Upload Stats: {string}", uploadStats );
            B2ThreadManager.B2ProcessStats.Add( uploadStats );

            _log?.LogInformation( "Concurrency Stats: {string}", concurrencyStats );
            B2ThreadManager.ConcurrencyStats.Add( concurrencyStats );

            _log?.LogInformation( "Thread UploadStats:" );
            B2ThreadManager.ShowThreadStatistics( true );

            foreach (FailureInfo failure in B2ThreadManager.FailureDetails) {
                failure.Reset( );
            }

            return uploadObject;
        }

    }
}
