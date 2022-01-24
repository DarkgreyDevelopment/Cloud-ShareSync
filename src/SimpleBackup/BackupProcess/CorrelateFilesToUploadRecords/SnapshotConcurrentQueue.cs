using System.Collections.Concurrent;
using System.Diagnostics;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static ConcurrentQueue<T> SnapshotConcurrentQueue<T>( ConcurrentQueue<T> queue ) {
            using Activity? activity = s_source.StartActivity(
                "ValidateExistingUploads.SnapshotConcurrentQueue" )?.Start( );

            s_logger?.ILog?.Debug( "Taking a snapshot of the existing queue." );

            ConcurrentQueue<T> results = new( );
            while (queue.TryDequeue( out T? item )) {
                results.Enqueue( item );
            }

            s_logger?.ILog?.Debug( "Snapshot complete." );

            activity?.Stop( );
            return results;
        }

    }
}
