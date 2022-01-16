using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Cloud_ShareSync.Core.Cryptography.FileEncryption.Types;

namespace Cloud_ShareSync.Core.Database.Entities {
    [Table( "Encryption" )]
    public class EncryptionTable {
        [Key]
        public long Id { get; set; }

        public string DecryptionData { get; set; }

        public EncryptionTable(
            long id,
            string decryptionData
        ) {
            Id = id;
            DecryptionData = decryptionData;
        }

        public EncryptionTable(
            long id,
            DecryptionData decryptionData
        ) {
            Id = id;
            DecryptionData = decryptionData.ToString( );
        }


        public override string ToString( ) {
            return JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
        }
    }
}
