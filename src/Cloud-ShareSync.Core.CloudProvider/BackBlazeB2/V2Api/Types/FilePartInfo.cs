using Cloud_ShareSync.Core.Cryptography;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    internal class FilePartInfo {

        internal FilePartInfo(
            FileInfo fileInfo,
            int partNumber,
            int contentLength,
            long offset,
            ILogger? log = null
        ) {
            PartNumber = partNumber;
            Data = new byte[contentLength];
            Hashing hasher = new( log );
            SHA1 = hasher.GetSha1Hash(
                fileInfo,
                Data,
                offset
            ).GetAwaiter( ).GetResult( );
        }

        public int PartNumber { get; private set; }
        public string SHA1 { get; private set; } = string.Empty;
        public byte[] Data { get; private set; }
    }
}
