using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Cloud_ShareSync.Core.Database.Entities {
    [Table( "BackBlazeB2" )]
    public class BackBlazeB2Table {
        [Key]
        public long Id { get; set; }

        [Column( "bucketName" )]
        public string BucketName { get; set; }

        [Column( "bucketId" )]
        public string BucketId { get; set; }

        [Column( "fileID" )]
        public string FileID { get; set; }

        public BackBlazeB2Table(
            long id,
            string bucketName,
            string bucketId,
            string fileID
        ) {
            Id = id;
            BucketName = bucketName;
            BucketId = bucketId;
            FileID = fileID;
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
