namespace Cloud_ShareSync.Core.FileSystemWatcher {
    public class EventData {
        public EventData(
            string fullPath,
            string? oldFullPath = null
        ) {
            FullPath = fullPath;
            OldFullPath = oldFullPath;
            EventTime = DateTime.Now;
        }

        public string FullPath { get; set; }
        public string? OldFullPath { get; set; }
        public DateTime EventTime { get; set; }
    }
}
