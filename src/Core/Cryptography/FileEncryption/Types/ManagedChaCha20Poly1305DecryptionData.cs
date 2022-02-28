using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cloud_ShareSync.Core.Cryptography.FileEncryption.Types {
    internal class ManagedChaCha20Poly1305DecryptionData {

        [JsonInclude]
        [JsonPropertyName( "Key" )]
        public readonly string Key; // Exclusively used in ToString Json.

        [JsonIgnore]
        public byte[] KeyBytes { get; private set; } // Ignore binary data for json, using _key string instead.
        public List<ManagedChaCha20Poly1305DecryptionKeyNote> KeyNoteList { get; private set; }

        public ManagedChaCha20Poly1305DecryptionData(
            byte[] key,
            List<ManagedChaCha20Poly1305DecryptionKeyNote> keyNoteList
        ) {
            KeyBytes = key;
            Key = Convert.ToBase64String( key, 0, key.Length );
            KeyNoteList = keyNoteList;
            ValidateLength( );
        }

        [JsonConstructor]
        public ManagedChaCha20Poly1305DecryptionData(
            string key,
            List<ManagedChaCha20Poly1305DecryptionKeyNote> keyNoteList
        ) {
            KeyBytes = Convert.FromBase64String( key );
            Key = key;
            KeyNoteList = keyNoteList;
            ValidateLength( );
        }

        private void ValidateLength( ) {
            if (KeyBytes.Length != 32) {
                throw new ArgumentOutOfRangeException(
                    nameof( KeyBytes ),
                    $"Key should be 32 bytes (256 bits). Current Length={KeyBytes.Length}"
                );
            }

            int ntpCount = 0;
            foreach (ManagedChaCha20Poly1305DecryptionKeyNote ntp in KeyNoteList) {
                // Check Tag Size
                if (ntp.Tag.Length != 16) {
                    throw new ArgumentOutOfRangeException(
                        "KeyNoteList.Tag",
                        $"KeyNoteList[{ntpCount}].Tag should be 16 bytes (128 bits). " +
                        $"Current Length: {ntp.Tag.Length}"
                    );
                }

                // Check Nonce Size
                if (ntp.Nonce.Length != 12) {
                    throw new ArgumentOutOfRangeException(
                        "KeyNoteList.Nonce",
                        $"KeyNoteList[{ntpCount}].Nonce should be 12 bytes (96 bits). " +
                        $"Current Length: {ntp.Nonce.Length}"
                    );
                }
                ntpCount++;
            }
        }

        public override string ToString( ) {
            return JsonSerializer.Serialize( this, new JsonSerializerOptions { WriteIndented = true } );
        }

        internal static ManagedChaCha20Poly1305DecryptionData Deserialize( FileInfo keyFile ) {
            if (File.Exists( keyFile.FullName )) {
                // Read KeyFile
                string? json = File.ReadAllText( keyFile.FullName );

                // Parse JsonDocument
                using JsonDocument document = JsonDocument.Parse( json );
                JsonElement root = document.RootElement;

                // Interpret Key
                string? key = null;
                if (root.TryGetProperty( nameof( KeyBytes ), out JsonElement keyElement )) {
                    key = keyElement.GetString( );
                }

                // Interpret KeyNoteList's
                List<ManagedChaCha20Poly1305DecryptionKeyNote> decryptionPairs = new( );
                if (root.TryGetProperty( nameof( KeyNoteList ), out JsonElement keyNote )) {

                    int ntpCount = 0;
                    foreach (JsonElement kn in keyNote.EnumerateArray( )) {
                        string? nonce = null;
                        string? tag = null;
                        int? order = null;

                        if (kn.TryGetProperty( "Nonce", out JsonElement nonceElement )) { nonce = nonceElement.GetString( ); }
                        if (kn.TryGetProperty( "Tag", out JsonElement tagElement )) { tag = tagElement.GetString( ); }
                        if (kn.TryGetProperty( "Order", out JsonElement orderElement )) { order = orderElement.GetInt32( ); }

                        if (
                            nonce != null &&
                            tag != null &&
                            order != null
                        ) {
                            decryptionPairs.Add( new( nonce, tag, (int)order ) );
                        } else {
                            throw new ArgumentOutOfRangeException(
                                nameof( keyFile ),
                                $"KeyNoteList[{ntpCount}] Is Invalid. Invalid Keyfile."
                            );
                        }

                        ntpCount++;
                    }
                }

                return (key != null && decryptionPairs.Count > 0) ?
                    new ManagedChaCha20Poly1305DecryptionData( key, decryptionPairs ) :
                    throw new ArgumentOutOfRangeException( nameof( keyFile ), "Invalid Keyfile." );

            } else {
                throw new ArgumentException( "KeyFile doesn't exist.", nameof( keyFile ) );
            }
        }
    }
}
