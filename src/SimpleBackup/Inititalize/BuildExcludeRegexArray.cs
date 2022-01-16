using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static Regex[] BuildExcludeRegexArray( string[] excludePaths ) {
            using Activity? activity = s_source.StartActivity( "Inititalize.BuildExcludeRegexArray" )?.Start( );

            List<Regex> regexPatterns = new( );
            foreach (string exPath in excludePaths) {
                regexPatterns.Add( new( exPath, RegexOptions.Compiled ) );
            }

            activity?.Stop( );
            return regexPatterns.ToArray( );
        }

    }
}
