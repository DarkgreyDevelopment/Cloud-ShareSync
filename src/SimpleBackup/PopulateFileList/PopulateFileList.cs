using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void PopulateFileList( ) {
            using Activity? activity = s_source.StartActivity( "PopulateFileList" )?.Start( );

            if (s_config?.SimpleBackup == null) { throw new InvalidDataException( "SimpleBackup config cannot be null" ); }


            string txt = s_config.SimpleBackup.MonitorSubDirectories ? " recursively " : " ";
            s_logger?.ILog?.Debug( $"Populating file list{txt}from root folder '{s_config?.SimpleBackup.RootFolder}'." );

            IEnumerable<string> files = s_config?.SimpleBackup.RootFolder == null ?
                Enumerable.Empty<string>( ) :
                Directory.EnumerateFiles(
                    s_config.SimpleBackup.RootFolder,
                    "*",
                    s_config.SimpleBackup.MonitorSubDirectories ?
                        SearchOption.AllDirectories :
                        SearchOption.TopDirectoryOnly
                );


            s_logger?.ILog?.Debug(
                $"Discovered {files.Count( )} files under '{s_config?.SimpleBackup.RootFolder}'. " +
                "Building file upload queue."
            );
            int count = 0;
            foreach (string path in files) {
                bool includePath = true;
                foreach (Regex pattern in s_excludePatterns) {
                    if (pattern.Match( path ).Success) {
                        includePath = false;
                        break;
                    }
                }

                if (includePath && s_fileUploadQueue.Contains( path ) == false) {
                    s_fileUploadQueue.Enqueue( path );
                } else {
                    s_logger?.ILog?.Debug( $"Skipping excluded file: '{path}'" );
                }
                count++;
            }
            s_logger?.ILog?.Debug( $"File upload queue contains {s_fileUploadQueue.Count} files." );

            activity?.Stop( );
        }

    }
}
