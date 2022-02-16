using Cloud_ShareSync.SimpleBackup.BackgroundService.Types;

namespace Cloud_ShareSync.SimpleBackup.BackgroundService.Interfaces {
    public interface IUploadFileProcess {
        Task Process( UploadFileInput ufInput );
    }
}
