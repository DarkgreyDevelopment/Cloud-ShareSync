using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static IEnumerable<string> ExcludeUnWantedPaths( IEnumerable<string> paths ) {
            using Activity? activity = s_source.StartActivity( "ExcludeUnWantedPaths" )?.Start( );

            List<string> result = new( );

            foreach (string path in paths) {
                bool includePath = true;
                foreach (Regex pattern in s_excludePatterns) {
                    Match m = pattern.Match( path );
                    if (m.Success) {
                        includePath = false;
                        break;
                    }
                }

                if (includePath) {
                    result.Add( path );
                } else {
                    s_logger?.ILog?.Debug( $"Skipping excluded file '{path}'." );
                }
            }

            activity?.Stop( );
            return result;
        }

    }
}
