using System.Collections.Concurrent;

namespace Cloud_ShareSync.Core.BackgroundService.DownloadFile {
    public interface IDownloadFileProcess {
        public static readonly ConcurrentQueue<DownloadFileInput> Queue = new( );
        Task Process( );
    }
}
