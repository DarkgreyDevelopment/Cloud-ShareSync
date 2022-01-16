using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cloud_ShareSync.Core.Database.Entities {
    public class PrimaryTable {

        [Key]
        public long Id { get; set; } // (Sequence number) (pkey)
        public string FileName { get; set; }
        public string UploadPath { get; set; }
        public string FileHash { get; set; } // Used for sync validation (eg, did this get uploaded already).
        public string UploadedFileHash { get; set; } // Used for download validation.
        public bool IsEncrypted { get; set; } // (what to do if IsEncrypted and ChaCha20Poly1305 encryption is not supported on platform?)
        public bool IsCompressed { get; set; } // (what to do if iscompressed and compression tools not available?)
        public bool UsesAwsS3 { get; set; }
        public bool UsesAzureBlobStorage { get; set; }
        public bool UsesBackBlazeB2 { get; set; }
        public bool UsesGoogleCloudStorage { get; set; }

        public PrimaryTable( ) {
            Id = 0;
            FileName = "";
            UploadPath = "";
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
            UploadPath = uploadpath;
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
