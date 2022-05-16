using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud_ShareSync.Core.Database.Entities {
    [Table( "GoogleCloudStorage" )]
    public class GoogleCloudStorageTable {
        [Key]
        public long Id { get; set; }

        public GoogleCloudStorageTable( ) { throw new NotImplementedException( ); }
    }
}
