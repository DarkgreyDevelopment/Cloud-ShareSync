using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud_ShareSync.Core.Database.Entities {
    [Table( "AzureBlobStorage" )]
    internal class AzureBlobStorageTable {
        [Key]
        public long Id { get; set; }

        public AzureBlobStorageTable( ) { throw new NotImplementedException( ); }
    }
}
