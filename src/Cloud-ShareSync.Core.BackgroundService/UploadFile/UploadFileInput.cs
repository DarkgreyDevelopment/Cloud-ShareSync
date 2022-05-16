using Cloud_ShareSync.Core.CloudProvider.Types;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.Core.BackgroundService.UploadFile {
    public class UploadFileInput {
        public readonly UploadFileInfo UploadFile;
        public readonly PrimaryTable TableData;

        public UploadFileInput(
            UploadFileInfo uploadFile,
            PrimaryTable tableData
        ) {
            UploadFile = uploadFile;
            TableData = tableData;
        }
    }
}
