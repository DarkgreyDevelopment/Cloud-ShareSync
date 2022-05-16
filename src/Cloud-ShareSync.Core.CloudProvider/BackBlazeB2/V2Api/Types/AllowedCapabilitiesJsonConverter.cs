using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    internal class AllowedCapabilitiesJsonConverter : JsonConverter<AuthorizeAccountAllowedCapabilities> {
        public override AuthorizeAccountAllowedCapabilities Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) {
            List<AuthorizeAccountAllowedCapabilities> enumList = ParseCapabiltiesArray( ref reader )
                .Select(
                    s =>
                    (AuthorizeAccountAllowedCapabilities)Enum.Parse(
                        typeof( AuthorizeAccountAllowedCapabilities ), s
                    )
                ).ToList( );
            AuthorizeAccountAllowedCapabilities returnVar = new( );
            foreach (AuthorizeAccountAllowedCapabilities e in enumList) { returnVar |= e; }
            return returnVar;
        }

        public override void Write(
            Utf8JsonWriter writer,
            AuthorizeAccountAllowedCapabilities capabilities,
            JsonSerializerOptions options
        ) => throw new NotImplementedException( "Failed to convert AuthorizeAccountAllowedCapabilities to a json string." );

        private static List<string> ParseCapabiltiesArray( ref Utf8JsonReader reader ) {
            List<string> results = new( );
            while (
                reader.Read( ) &&
                reader.TokenType == JsonTokenType.String
            ) {
                results.Add( reader.GetString( )! );
            }
            return results;
        }
    }
}
