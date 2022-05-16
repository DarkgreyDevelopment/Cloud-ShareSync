using System.Collections.Concurrent;

namespace Cloud_ShareSync.Core.BackgroundService.PrepFile {
    public interface IPrepFileProcess {
        public static readonly ConcurrentQueue<PrepItem> Queue = new( );
        Task Prep( List<string> paths );
        Task ProcessBackup( );
        Task ProcessRestore( );
    }
}
