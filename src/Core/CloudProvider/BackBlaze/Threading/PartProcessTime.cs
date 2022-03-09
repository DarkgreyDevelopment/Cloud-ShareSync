namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading {

    internal class PartProcessTime {

        private readonly object _lock = new( );
        public readonly int PartNumber;

        private readonly Dictionary<DateTime, DateTime?> _processTimes;

        public TimeSpan? _processTime = null;

        public PartProcessTime( int partNumber ) {
            PartNumber = partNumber;
            _processTimes = new( );
        }

        public void SetProcessTime( ) {
            lock (_lock) {
                List<TimeSpan> timespans = new( );
                foreach (KeyValuePair<DateTime, DateTime?> t in _processTimes) {
                    if (t.Value != null) {
                        timespans.Add( t.Key - (DateTime)t.Value );
                    }
                }
                double doubleAverageTicks = timespans.Average( timeSpan => timeSpan.Ticks );
                long longAverageTicks = Convert.ToInt64( doubleAverageTicks );
                _processTime = longAverageTicks > 0 ? new TimeSpan( longAverageTicks ) : null;
            }
        }

        public void AddNewStartTime( ) {
            lock (_lock) {
                _processTimes.Add( DateTime.Now, null );
            }
        }

        public void AddNewStopTime( ) {
            lock (_lock) {
                KeyValuePair<DateTime, DateTime?> t = _processTimes.Where( e => e.Value == null ).First( );
                _processTimes[t.Key] = DateTime.Now;
            }
        }
    }
}
