using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.Configuration.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void PopulateFileList(
            ConcurrentQueue<string> fileUploadQueue,
            Regex[] excludePatterns,
            BackupConfig config,
            ILogger? log = null
        ) {
            using Activity? activity = s_source.StartActivity( "PopulateFileList" )?.Start( );

            IEnumerable<string> files = EnumerateRootFolder( config, log );

            log?.LogInformation( "Building file upload queue." );
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
                    log?.LogDebug( "Skipping excluded file: '{string}'", path );
                }
                count++;
            }

            log?.LogInformation( "File upload queue contains {int} files.", fileUploadQueue.Count );
            activity?.Stop( );
        }

        private static IEnumerable<string> EnumerateRootFolder(
            BackupConfig config,
            ILogger? log = null
        ) {
            using Activity? activity = s_source.StartActivity( "EnumerateRootFolder" )?.Start( );

            string txt = config.MonitorSubDirectories ? " recursively " : " ";
            log?.LogInformation( "Populating file list{string}from root folder '{string}'.", txt, config.RootFolder );

            IEnumerable<string> files = config.RootFolder == null ?
                Enumerable.Empty<string>( ) :
                Directory.EnumerateFiles(
                    config.RootFolder,
                    "*",
                    config.MonitorSubDirectories ?
                        SearchOption.AllDirectories :
                        SearchOption.TopDirectoryOnly
                );
            log?.LogInformation( "Discovered {int} files under '{string}'.", files.Count( ), config.RootFolder );

            activity?.Stop( );
            return files;
        }
    }
}
