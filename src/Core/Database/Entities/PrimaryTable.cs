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
        public bool UsesAwsS3 { get; set; }
        public bool UsesAzureBlobStorage { get; set; }
        public bool UsesBackBlazeB2 { get; set; }
        public bool UsesGoogleCloudStorage { get; set; }

        public PrimaryTable( ) {
            Id = 0;
            FileName = "";
            RelativeUploadPath = "";
            FileHash = "";
            UploadedFileHash = "";
            IsEncrypted = false;
            IsCompressed = false;
            UsesAwsS3 = false;
            UsesAzureBlobStorage = false;
            UsesBackBlazeB2 = false;
            UsesGoogleCloudStorage = false;
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
            UsesAwsS3 = aws;
            UsesAzureBlobStorage = azure;
            UsesBackBlazeB2 = backblaze;
            UsesGoogleCloudStorage = gcs;
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
