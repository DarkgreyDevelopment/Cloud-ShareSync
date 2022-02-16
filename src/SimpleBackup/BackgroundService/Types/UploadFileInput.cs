using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.SimpleBackup.BackgroundService.Types {
    public class UploadFileInput {
        public FileInfo UploadFile { get; set; }
        public string RelativePath { get; set; }
        public PrimaryTable TableData { get; set; }

        public UploadFileInput(
            FileInfo uploadFile,
            string relativePath,
            PrimaryTable tableData
        ) {
            UploadFile = uploadFile;
            RelativePath = relativePath;
            TableData = tableData;
        }
    }
}
