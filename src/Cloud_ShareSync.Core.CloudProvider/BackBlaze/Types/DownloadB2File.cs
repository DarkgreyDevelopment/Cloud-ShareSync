using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.Interfaces;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    public class DownloadB2File : ICloudProviderDownload {

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

        public FileInfo OutputPath { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; } = "";

        public override string ToString( ) {
            JsonSerializerOptions options = new( ) {
                IncludeFields = true,
                WriteIndented = true,
            };
            return JsonSerializer.Serialize( this, options );
        }
    }
}
