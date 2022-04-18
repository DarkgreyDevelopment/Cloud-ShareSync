using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading {
    internal class B2ProcessStats {
        private readonly ILogger? _log;
        private readonly DateTime _startTime;
        private DateTime? _stopTime;

        internal readonly long FileLength;
        internal double BytesPerMillisecond =>
            (ProcessTime == null) ? 0 : (FileLength / ProcessTime.Value.TotalMilliseconds);

        public TimeSpan? ProcessTime => (_stopTime == null) ? null : _stopTime - _startTime;

        public double MibibytesPerSecond =>
            (ProcessTime == null) ? 0 : (FileSizeMB / ProcessTime.Value.TotalSeconds);

        public double FileSizeMB => FileLength / (1024 * 1024);

        public B2ProcessStats( long length, ILogger? log ) {
            _startTime = DateTime.Now;
            FileLength = length;
            _log = log;
        }

        public void SetStopTime( ) {
            _stopTime = DateTime.Now;
            _log?.LogInformation( "Uploaded Large File Parts Async" );
        }

        public override string ToString( ) {
            return JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
        }
    }
}
