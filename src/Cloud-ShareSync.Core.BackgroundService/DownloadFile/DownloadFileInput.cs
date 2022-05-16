using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.Core.BackgroundService.DownloadFile {
    public class DownloadFileInput {

        public DownloadFileInput(
            FileInfo downloadFile,
            PrimaryTable tableData
        ) {
            FilePath = downloadFile;
            TableData = tableData;
        }

        [JsonConverter( typeof( FileSystemInfoJsonConverter ) )]
        public readonly FileInfo FilePath;
        public readonly PrimaryTable TableData;
        public AwsS3Table? AwsData { get; set; }
        public AzureBlobStorageTable? AzureData { get; set; }
        public BackBlazeB2Table? BackBlazeData { get; set; }
        public GoogleCloudStorageTable? GoogleData { get; set; }
        public EncryptionTable? EncryptionData { get; set; }
        public CompressionTable? CompressionData { get; set; }
    }
}
