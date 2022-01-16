using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cloud_ShareSync.Core.Cryptography.FileEncryption.Types {
    public class DecryptionKeyNote {

        public byte[] Nonce { get; private set; }
        public byte[] Tag { get; private set; }
        public int Order { get; private set; }

        public DecryptionKeyNote(
            byte[] nonce,
            byte[] tag,
            int order
        ) {
            Nonce = nonce;
            Tag = tag;
            Order = order;
        }

        [JsonConstructor]
        public DecryptionKeyNote(
            string nonce,
            string tag,
            int order
        ) {
            Nonce = Convert.FromBase64String( nonce );
            Tag = Convert.FromBase64String( tag );
            Order = order;
        }

        public override string ToString( ) {
            return JsonSerializer.Serialize(
                new DecryptionKeyNoteBase64Stringified( Nonce, Tag, Order ),
                new JsonSerializerOptions { WriteIndented = true }
            );
        }

        // Private Helper Class to convert bytes ToBase64 string.
        private class DecryptionKeyNoteBase64Stringified {
            internal DecryptionKeyNoteBase64Stringified(
                byte[] nonce,
                byte[] tag,
                int order
            ) {
                Nonce = Convert.ToBase64String( nonce, 0, nonce.Length );
                Tag = Convert.ToBase64String( tag, 0, tag.Length );
                Order = order;
            }

            internal string Nonce { get; set; }
            internal string Tag { get; set; }
            internal int Order { get; set; }
        }
    }
}
