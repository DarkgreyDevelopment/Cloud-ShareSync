using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Cloud_ShareSync.Core.Database.Entities {
    [Table( "Compression" )]
    public class CompressionTable {
        [Key]
        public long Id { get; set; }
        public bool PasswordProtected { get; set; }
        public string? Password { get; set; }

        public CompressionTable(
            long id,
            bool passwordProtected = false,
            string? password = null
        ) {
            Id = id;
            PasswordProtected = passwordProtected;
            Password = password;
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
