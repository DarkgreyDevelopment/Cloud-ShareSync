using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cloud_ShareSync.Core.Database.Entities {
    public class PrimaryTable {

        ///<value>Sequence number and Primary Key</value>
        [Key]
        public long Id { get; set; }

        ///<value>Stores the FileInfo.Name property of the file</value>
        public string FileName { get; set; }

        ///<value>Stores the FileInfo.FullName relative to the RootFolder.</value>
        public string RelativeUploadPath { get; set; }

        ///<value>Used for sync validation.<br/>
        /// Used to see if the file has been uploaded already.<br/>
        /// Also ensures downloaded, decrypted, and decompressed file matches original file.
        /// </value>
        public string FileHash { get; set; }

        ///<value>Used to ensure downloaded file matches uploaded file.</value>
        public string UploadedFileHash { get; set; }

        public bool IsEncrypted { get; set; } // (what to do if IsEncrypted and ChaCha20Poly1305 encryption is not supported on platform?)
        public bool IsCompressed { get; set; } // (what to do if iscompressed and compression tools not available?)
        public bool StoredInAwsS3 { get; set; }
        public bool StoredInAzureBlobStorage { get; set; }
        public bool StoredInBackBlazeB2 { get; set; }
        public bool StoredInGoogleCloudStorage { get; set; }

        public PrimaryTable( ) {
            Id = 0;
            FileName = "";
            RelativeUploadPath = "";
            FileHash = "";
            UploadedFileHash = "";
            IsEncrypted = false;
            IsCompressed = false;
            StoredInAwsS3 = false;
            StoredInAzureBlobStorage = false;
            StoredInBackBlazeB2 = false;
            StoredInGoogleCloudStorage = false;
        }

        public PrimaryTable(
            string filename,
            string uploadpath,
            string hash,
            string uploadhash,
            bool encrypted,
            bool compressed,
            bool aws,
            bool azure,
            bool backblaze,
            bool gcs
        ) {

            Id = 00000000;
            FileName = filename;
            RelativeUploadPath = uploadpath;
            FileHash = hash;
            UploadedFileHash = uploadhash;
            IsEncrypted = encrypted;
            IsCompressed = compressed;
            StoredInAwsS3 = aws;
            StoredInAzureBlobStorage = azure;
            StoredInBackBlazeB2 = backblaze;
            StoredInGoogleCloudStorage = gcs;
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
