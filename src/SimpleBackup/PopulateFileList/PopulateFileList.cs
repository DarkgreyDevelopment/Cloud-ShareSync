using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.Configuration.Types;
using log4net;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void PopulateFileList(
            ConcurrentQueue<string> fileUploadQueue,
            Regex[] excludePatterns,
            BackupConfig config,
            ILog? log = null
        ) {
            using Activity? activity = s_source.StartActivity( "PopulateFileList" )?.Start( );

            IEnumerable<string> files = EnumerateRootFolder( config, log );

            log?.Info( "Building file upload queue." );
            int count = 0;
            foreach (string path in files) {
                bool includePath = true;
                foreach (Regex pattern in excludePatterns) {
                    if (pattern.Match( path ).Success) {
                        includePath = false;
                        break;
                    }
                }

                if (includePath && fileUploadQueue.Contains( path ) == false) {
                    fileUploadQueue.Enqueue( path );
                } else {
                    log?.Debug( $"Skipping excluded file: '{path}'" );
                }
                count++;
            }

            log?.Info( $"File upload queue contains {fileUploadQueue.Count} files." );
            activity?.Stop( );
        }

        private static IEnumerable<string> EnumerateRootFolder(
            BackupConfig config,
            ILog? log = null
        ) {
            using Activity? activity = s_source.StartActivity( "EnumerateRootFolder" )?.Start( );

            string txt = config.MonitorSubDirectories ? " recursively " : " ";
            log?.Info( $"Populating file list{txt}from root folder '{config.RootFolder}'." );

            IEnumerable<string> files = config.RootFolder == null ?
                Enumerable.Empty<string>( ) :
                Directory.EnumerateFiles(
                    config.RootFolder,
                    "*",
                    config.MonitorSubDirectories ?
                        SearchOption.AllDirectories :
                        SearchOption.TopDirectoryOnly
                );
            log?.Info( $"Discovered {files.Count( )} files under '{config.RootFolder}'." );

            activity?.Stop( );
            return files;
        }
    }
}
