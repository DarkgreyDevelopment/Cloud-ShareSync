using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cloud_ShareSync.Core.Database.Entities {
    [Table( "AwsS3" )]
    public class AwsS3Table {
        [Key]
        public long Id { get; set; }

        public AwsS3Table( ) { throw new NotImplementedException( ); }
    }
}
