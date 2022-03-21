using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Cloud_ShareSync.Core.SharedServices {
    internal static class MimeType {

        private static readonly ActivitySource s_source = new( "MimeType" );
        private static readonly Dictionary<string, string> s_mimeTypes = new( );

        static MimeType( ) { SetMimeTypes( ); }

        internal static string GetMimeTypeByExtension( FileInfo path ) {
            using Activity? activity = s_source.StartActivity( "GetMimeTypeByExtension" )?.Start( );

            string? mimeTypeFromFileExtension = s_mimeTypes
                    .FirstOrDefault( x => x.Key == path.Extension.ToLower( ) ).Value;

            activity?.Stop( );
            return mimeTypeFromFileExtension ?? "application/octet-stream";
        }

        private static void SetMimeTypes( ) {
            using Activity? activity = s_source.StartActivity( "GetMimeTypes" )?.Start( );

            // Determine path
            Assembly assembly = Assembly.GetExecutingAssembly( );
            string resourceName = assembly.GetManifestResourceNames( )
                                    .Single( str => str.EndsWith( "MimeTypes.json" ) );
            using Stream? stream = assembly.GetManifestResourceStream( resourceName );

            foreach (KeyValuePair<string, string> kvp in ReadMimeTypeDictionary( stream )) {
                _ = s_mimeTypes.TryAdd( kvp.Key, kvp.Value );
            }

            activity?.Stop( );
        }

        private static Dictionary<string, string> ReadMimeTypeDictionary( Stream? stream ) {
            if (stream != null) {
                Dictionary<string, string>? tmpDict = null;
                using StreamReader reader = new( stream );
                string jsonMimeTypes = reader.ReadToEnd( );
                tmpDict = JsonSerializer
                            .Deserialize<Dictionary<string, string>>(
                                jsonMimeTypes,
                                new JsonSerializerOptions( ) {
                                    ReadCommentHandling = JsonCommentHandling.Skip
                                }
                            );
                if (tmpDict != null) { return tmpDict; }
            }

            throw new InvalidOperationException(
                "Failed to deserialize embedded mimetypes file."
            );
        }
    }
}
