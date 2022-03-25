using System.Text.RegularExpressions;

namespace Cloud_ShareSync {

    public class Program {

        internal const string BackupPattern = "[bB][aA]?[cC]?[kK]?[uU]?[pP]?";
        internal const string RestorePattern = "[rR][eE]?[sS]?[tT]?[oO]?[rR]?[eE]?";

        internal static void Main( string[] args ) {
            List<string> arguments = args.ToList( );

            bool showHelp = true;

            if (arguments.Any( )) {
                string arg0 = arguments[0];
                string arg1 = arguments.Count > 1 ? arguments[1] : "";
                if (MatchesBackup( arg0, arg1 )) {
                    showHelp = false;
                    Backup.Program
                        .Main( PassArguments( arguments, arg1 ) )
                        .GetAwaiter( )
                        .GetResult( );
                } else if (MatchesRestore( arg0, arg1 )) {
                    showHelp = false;
                }
            }

            if (showHelp) { ShowHelp( ); }
        }

        internal static bool MatchesBackup( string arg0, string arg1 ) =>
            Regex.IsMatch( arg0, BackupPattern, RegexOptions.CultureInvariant ) ||
            Regex.IsMatch( arg1, BackupPattern, RegexOptions.CultureInvariant );

        internal static bool MatchesRestore( string arg0, string arg1 ) =>
            Regex.IsMatch( arg0, RestorePattern, RegexOptions.CultureInvariant ) ||
            Regex.IsMatch( arg1, RestorePattern, RegexOptions.CultureInvariant );

        internal static bool MatchesArg1( string arg1 ) =>
            Regex.IsMatch( arg1, BackupPattern, RegexOptions.CultureInvariant ) ||
            Regex.IsMatch( arg1, RestorePattern, RegexOptions.CultureInvariant );

        internal static void ShowHelp( ) { Console.WriteLine( "HELP!!!" ); }

        internal static string[] PassArguments( List<string> arguments, string arg1 ) {
            if (MatchesArg1( arg1 )) { _ = arguments.Remove( arg1 ); }
            return arguments.ToArray( );
        }
    }
}
