using System.Diagnostics;
using System.Text.Json;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        // All Paths are relative to the bucket root.
        private string CleanUploadPath( string uploadFilePath, bool json ) {
            using Activity? activity = _source.StartActivity( "CleanUploadPath" )?.Start( );

            string uploadpath = uploadFilePath
                .Replace( "\\", "/" )
                .TrimStart( '/' );
            string cleanUri = "";

            if (json) {
                cleanUri = JsonSerializer
                            .Serialize( uploadpath, new JsonSerializerOptions( ) )
                            .Replace( "\\u0022", null )
                            .Replace( "\"", null )
                            .Replace( "\u0022", null );

            } else {
                List<char> dontEscape = new( );
                dontEscape.AddRange( "._-/~!$'()*;=:@ ".ToCharArray( ) );
                dontEscape.AddRange( "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray( ) );
                dontEscape.AddRange( "abcdefghijklmnopqrstuvwxyz".ToCharArray( ) );
                dontEscape.AddRange( "0123456789".ToCharArray( ) );

                foreach (char c in uploadpath.ToCharArray( )) {
                    cleanUri += dontEscape.Contains( c ) ? c : Uri.HexEscape( c );
                }
                cleanUri = cleanUri.Replace( ' ', '+' );
            }
            cleanUri = cleanUri.TrimEnd( '/' );

            activity?.Stop( );
            return cleanUri;
        }
    }
}
