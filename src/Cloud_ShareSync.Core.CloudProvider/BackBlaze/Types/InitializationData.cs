using System.Text;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class InitializationData {
        internal int MaxErrors { get; set; }
        internal int UploadThreads { get; set; }
        internal string BucketName { get; set; }
        internal string BucketId { get; set; }
        internal string Credentials { get; set; }

        internal InitializationData(
            string applicationKeyId,
            string applicationKey,
            int maxErrors,
            int uploadThreads,
            string bucketName,
            string bucketId
        ) {
            MaxErrors = maxErrors;
            UploadThreads = uploadThreads;
            BucketName = bucketName;
            BucketId = bucketId;
            Credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes( $"{applicationKeyId}:{applicationKey}" )
            );
        }
    }
}
