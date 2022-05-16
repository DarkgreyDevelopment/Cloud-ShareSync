using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Cryptography;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.Types {
    public class UploadFileInfo {
        public UploadFileInfo(
            FileInfo filePath,
            string uploadFilePath,
            ILogger? log = null
        ) {
            Hashing hasher = new( log );
            FilePath = filePath;
            MimeType = SharedServices.MimeType.GetMimeTypeByExtension( filePath );
            SHA512 = hasher.GetSha512Hash( filePath ).GetAwaiter( ).GetResult( );
            UploadFilePath = uploadFilePath;
            LastWriteTimeMS = new DateTimeOffset( FilePath.LastWriteTimeUtc ).ToUnixTimeMilliseconds( );
        }

        [JsonConverter( typeof( FileSystemInfoJsonConverter ) )]
        public FileInfo FilePath { get; set; }
        public string UploadFilePath { get; set; }
        public string MimeType { get; private set; }
        public string SHA512 { get; private set; }
        public string SHA1 { get; set; } = string.Empty;
        public long LastWriteTimeMS { get; private set; }

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
