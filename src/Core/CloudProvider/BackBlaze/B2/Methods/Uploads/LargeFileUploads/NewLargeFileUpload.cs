using System.Collections.Concurrent;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

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

            int threadCount = totalParts < ThreadManager.ActiveThreadCount ?
                                totalParts :
                                ThreadManager.ActiveThreadCount;
            B2ConcurrentStats concurrencyStats = new( threadCount );
            _log?.Info( "Uploading Large File Parts Async" );
            _log?.Info( $"Splitting file into {totalParts - 1} {recSize} byte chunks and 1 {finalLength} chunk." );
            _log?.Info( $"Chunks will be uploaded asyncronously via {threadCount} upload streams." );

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
                _log?.Fatal( $"filePartQueue part length total does not equal files length." );
                throw new InvalidOperationException( "Failed to upload full file." );
            }

            List<Task<bool>> uploadTasks = new( );

            for (int thread = 0; thread < threadCount; thread++) {
                _log?.Debug( $"Thread#{thread} - Adding Task to Task List." );
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
            _log?.Info( "Uploaded Large File Parts Async" );

            _log?.Info( "Finishing Large File Upload." );
            if (resultsList != null) {
                foreach (LargeFilePartReturn result in resultsList) {
                    uploadObject.Sha1PartsList.Add( new( result.PartNumber, result.Sha1Hash ) );
                    uploadObject.TotalBytesSent += result.DataSize;
                }
            }

            _log?.Debug( $"Concurrency Stats: {concurrencyStats}" );
            ThreadManager.ConcurrencyStats.Add( concurrencyStats );

            _log?.Debug( "Thread UploadStats:" );
            ThreadManager.ShowThreadStatistics( true );


            await FinishUploadLargeFile( uploadObject );

            foreach (FailureInfo failure in ThreadManager.FailureDetails) {
                failure.Reset( );
            }

            return uploadObject;
        }

    }
}
