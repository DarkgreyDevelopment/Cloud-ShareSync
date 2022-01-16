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
        public bool SpecialDecompress { get; set; }
        public string? DecompressionArgs { get; set; }

        public CompressionTable(
            long id,
            bool passwordProtected = false,
            string? password = null,
            bool specialDecompress = false,
            string? decompressionArgs = null
        ) {
            Id = id;
            PasswordProtected = passwordProtected;
            Password = password;
            SpecialDecompress = specialDecompress;
            DecompressionArgs = decompressionArgs;
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
