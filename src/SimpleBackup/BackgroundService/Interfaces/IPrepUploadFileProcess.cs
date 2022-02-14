using System.Collections.Concurrent;

namespace Cloud_ShareSync.SimpleBackup.Interfaces {
    public interface IPrepUploadFileProcess {
        Task Prep( ConcurrentQueue<string> queue );
    }
}
