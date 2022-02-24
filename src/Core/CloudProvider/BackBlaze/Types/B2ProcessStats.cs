using System.Text.Json;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class B2ProcessStats {
        private readonly DateTime _startTime;
        private DateTime? _stopTime = null;

        public readonly long FileLength;
        public TimeSpan? ProcessTime => (_stopTime == null) ? null : _stopTime - _startTime;

        public double BytesPerMillisecond =>
            (ProcessTime == null) ? 0 : (FileLength / ProcessTime.Value.TotalMilliseconds);

        public B2ProcessStats( long length ) {
            _startTime = DateTime.Now;
            FileLength = length;
        }

        public void SetStopTime( ) { _stopTime = DateTime.Now; }

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
