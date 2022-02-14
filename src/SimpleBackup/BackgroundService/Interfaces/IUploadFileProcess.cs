using Cloud_ShareSync.SimpleBackup.Types;

namespace Cloud_ShareSync.SimpleBackup.Interfaces {
    public interface IUploadFileProcess {
        Task Process( UploadFileInput ufInput );
    }
}
