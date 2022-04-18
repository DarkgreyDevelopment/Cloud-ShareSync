using System.Collections.Concurrent;
using Cloud_ShareSync.SharedServices.BackgroundService.Types;

namespace Cloud_ShareSync.SharedServices.BackgroundService.Interfaces {
    public interface IUploadFileProcess {
        public static readonly ConcurrentQueue<UploadFileInput> Queue = new( );
        Task Process( );
    }
}
