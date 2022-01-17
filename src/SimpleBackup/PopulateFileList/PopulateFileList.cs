using System.Diagnostics;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void PopulateFileList( ) {
            using Activity? activity = s_source.StartActivity( "PopulateFileList" )?.Start( );

            if (s_config?.SimpleBackup == null) { throw new InvalidDataException( "SimpleBackup config cannot be null" ); }

            IEnumerable<string> files = s_config?.SimpleBackup.RootFolder == null ?
                Enumerable.Empty<string>( ) :
                Directory.EnumerateFiles(
                    s_config.SimpleBackup.RootFolder,
                    "*",
                    s_config.SimpleBackup.MonitorSubDirectories ?
                        SearchOption.AllDirectories :
                        SearchOption.TopDirectoryOnly
                );
            int count = 0;
            IEnumerable<string> enqueueFiles = ExcludeUnWantedPaths( files );
            string fileCountFormat = $"D{enqueueFiles.Count( ).ToString( ).Length}";

            foreach (string file in enqueueFiles) {
                if (s_fileUploadQueue.Contains( file ) == false) {
                    s_logger?.ILog?.Debug( $"Enqueueing File{count.ToString( fileCountFormat )}: {file}" );
                    s_fileUploadQueue.Enqueue( file );
                }
                count++;
            }
            activity?.Stop( );
        }

    }
}
