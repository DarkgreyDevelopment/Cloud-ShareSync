using System.Diagnostics;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static async Task BackupProcess( ) {
            using Activity? activity = s_source.StartActivity( "BackupProcess" )?.Start( );

            while (s_fileUploadQueue.IsEmpty == false) {
                bool deQueue = s_fileUploadQueue.TryDequeue( out string? path );

                if (deQueue && path != null) { await UploadFileProcess( path ); }
            }

            activity?.Stop( );
            return;
        }

    }
}
