using System.Text;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    internal class InitializationData {

        internal InitializationData(
            string applicationKeyId,
            string applicationKey,
            string bucketName,
            string bucketId,
            int maxErrors,
            int uploadThreads
        ) {
            Credentials = CreateCredential( applicationKeyId, applicationKey );
            BucketName = bucketName;
            BucketId = bucketId;
            MaxErrors = maxErrors;
            HttpThreads = uploadThreads;
        }

        internal readonly string Credentials;
        internal readonly string BucketName;
        internal readonly string BucketId;
        internal readonly int MaxErrors;
        internal readonly int HttpThreads;

        private static string CreateCredential(
            string applicationKeyId,
            string applicationKey
        ) => Convert.ToBase64String(
            Encoding.UTF8.GetBytes( $"{applicationKeyId}:{applicationKey}" )
        );

    }
}
