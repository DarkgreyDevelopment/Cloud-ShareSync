using System.Text.Json;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api {
    internal static class B2PathEscaper {

        public static string CleanUploadPath( string uploadFilePath, bool json ) {
            string uploadpath = uploadFilePath
                .Replace( "\\", "/" )
                .TrimStart( '/' );
            string cleanUri = (json) ?
                GetJsonUri( uploadpath ) :
                GetStringUri( uploadpath.ToCharArray( ) );
            cleanUri = cleanUri.TrimEnd( '/' );
            return cleanUri;
        }

        private static readonly char[] s_dontEscapeChars = SetDontEscapeChars( );

        private static char[] SetDontEscapeChars( ) {
            List<char> dontEscape = new( );
            dontEscape.AddRange( "._-/~!$'()*;=:@ ".ToCharArray( ) );
            dontEscape.AddRange( "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray( ) );
            dontEscape.AddRange( "abcdefghijklmnopqrstuvwxyz".ToCharArray( ) );
            dontEscape.AddRange( "0123456789".ToCharArray( ) );
            return dontEscape.ToArray( );
        }

        private static string GetStringUri( char[] uploadPathCharArray ) {
            string result = string.Empty;
            foreach (char c in uploadPathCharArray) {
                result += s_dontEscapeChars.Contains( c ) ? c : Uri.HexEscape( c );
            }
            return result.Replace( ' ', '+' );
        }


        private static string GetJsonUri( string uploadpath ) =>
            ReplaceUnwantedJsonValues(
                JsonSerializer
                .Serialize( uploadpath, new JsonSerializerOptions( ) )
            );

        private static string ReplaceUnwantedJsonValues( string json ) =>
            json.Replace( "\\u0022", null )
                .Replace( "\"", null )
                .Replace( "\u0022", null );
    }
}
