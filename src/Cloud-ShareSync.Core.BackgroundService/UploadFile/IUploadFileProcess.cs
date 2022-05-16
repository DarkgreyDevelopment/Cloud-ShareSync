using System.Collections.Concurrent;

namespace Cloud_ShareSync.Core.BackgroundService.UploadFile {
    public interface IUploadFileProcess {
        public static readonly ConcurrentQueue<UploadFileInput> Queue = new( );
        Task Process( );
    }
}
