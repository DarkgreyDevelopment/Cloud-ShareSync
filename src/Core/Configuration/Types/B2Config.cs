using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Interfaces;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    public class B2Config : ICloudProviderConfig {
        public string ApplicationKeyId { get; set; }
        public string ApplicationKey { get; set; }
        public string BucketName { get; set; }
        public string BucketId { get; set; }
        public int MaxConsecutiveErrors { get; set; } = 5;
        public int UploadThreads { get; set; } = 25;

        // Public Parameterless constructor 
        public B2Config( ) { }

        public B2Config(
            string applicationKeyId,
            string applicationKey,
            string bucketName,
            string bucketId,
            int maxErrors,
            int uploadThreads
        ) {
            ApplicationKeyId = applicationKeyId;
            ApplicationKey = applicationKey;
            BucketName = bucketName;
            BucketId = bucketId;
            MaxConsecutiveErrors = maxErrors;
            UploadThreads = uploadThreads;
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
