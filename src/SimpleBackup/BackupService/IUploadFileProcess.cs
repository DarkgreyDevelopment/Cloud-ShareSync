using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.SimpleBackup.BackupService {
    public interface IUploadFileProcess {
        Task Process( FileInfo uploadFile, string uploadPath, PrimaryTable tabledata );
    }
}
