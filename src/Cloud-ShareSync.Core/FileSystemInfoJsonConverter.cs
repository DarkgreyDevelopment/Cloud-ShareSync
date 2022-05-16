using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cloud_ShareSync.Core {
    public class FileSystemInfoJsonConverter : JsonConverter<FileSystemInfo> {
        public override FileSystemInfo Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) {
            string fullname = ParseFullName( ref reader );
            return (typeToConvert == typeof( FileInfo ))
                 ? new FileInfo( fullname )
                 : new DirectoryInfo( fullname );
        }

        public override void Write(
            Utf8JsonWriter writer,
            FileSystemInfo fileInfo,
            JsonSerializerOptions options
        ) => writer.WriteStringValue( fileInfo.FullName );


        private static string ParseFullName( ref Utf8JsonReader reader ) =>
            (reader.Read( ) && reader.TokenType == JsonTokenType.String)
                ? reader.GetString( )!
                : "\\**/Unknown FileSystemInfo\\**/"; // This will fail on the new FileInfo/DirectoryInfo creation.
    }
}
