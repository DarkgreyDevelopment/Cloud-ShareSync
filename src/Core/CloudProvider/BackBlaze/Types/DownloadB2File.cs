using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.Interface;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class DownloadB2File : ICloudProviderDownload {

        public FileInfo OutputPath { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; } = "";

        public DownloadB2File(
            FileInfo outputPath,
            string fileId
        ) {
            OutputPath = outputPath;
            FileId = fileId;
        }

        public DownloadB2File(
            string fileName,
            FileInfo outputPath
        ) {
            OutputPath = outputPath;
            FileId = "";
            FileName = fileName;
        }

        public override string ToString( ) {
            JsonSerializerOptions options = new( ) {
                IncludeFields = true,
                WriteIndented = true,
            };
            return JsonSerializer.Serialize( this, options );
        }
    }
}
