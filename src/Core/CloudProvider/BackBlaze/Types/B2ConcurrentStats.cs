using Cloud_ShareSync.CloudProvider.BackBlaze.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class B2ConcurrentStats {

        #region Fields

        private readonly object _lock = new( );

        public readonly int Available;
        public int Started => _started.Count;
        private readonly List<int> _started;

        public int Active => _active.Count;
        private readonly List<int> _active;

        public int Sleeping => _sleeping.Count;
        private readonly List<int> _sleeping;

        public int Failed => _failed.Count;
        private readonly List<int> _failed;

        public int Completed => _completed.Count;
        private readonly List<int> _completed;

        public int HighWaterActive { get; private set; } = 0;
        public int HighWaterSleeping { get; private set; } = 0;

        private readonly ILogger? _log;

        #endregion Fields


        #region Ctor

        public B2ConcurrentStats( int available, ILogger? log ) {
            Available = available;
            _started = new( );
            _active = new( );
            _failed = new( );
            _sleeping = new( );
            _completed = new( );
            _log = log;
        }

        #endregion Ctor


        #region Methods

        public override string ToString( ) {
            return "\n" +
            $"Available:         {Available}\n" +
            $"Started:           {Started}\n" +
            $"Active:            {Active}\n" +
            $"HighWaterActive:   {HighWaterActive}\n" +
            $"Sleeping:          {Sleeping}\n" +
            $"HighWaterSleeping: {HighWaterSleeping}\n" +
            $"Failed:            {Failed}\n" +
            $"Completed:         {Completed}";
        }

        #region private and locked.

        private void SetHighWaterMarks( ) {
            lock (_lock) {
                HighWaterActive = Active > HighWaterActive ? Active : HighWaterActive;
                HighWaterSleeping = Sleeping > HighWaterSleeping ? Sleeping : HighWaterSleeping;
            }
        }

        private void AddStartThread( int thread ) {
            lock (_lock) { _started.Add( thread ); }
        }

        private void AddActive( int thread ) {
            lock (_lock) { _active.Add( thread ); }
        }

        private void AddCompleted( int thread ) {
            lock (_lock) { _completed.Add( thread ); }
        }

        private void AddSleeping( int thread ) {
            lock (_lock) { _sleeping.Add( thread ); }
        }

        private void AddFailed( int thread ) {
            lock (_lock) { _failed.Add( thread ); }
        }

        private void RemoveStarted( int thread ) {
            lock (_lock) {
                if (_started.Contains( thread )) {
                    _started.Remove( thread );
                }
            }
        }

        private void RemoveActive( int thread ) {
            lock (_lock) {
                if (_active.Contains( thread )) {
                    _active.Remove( thread );
                }
            }
        }

        private void RemoveSleeping( int thread ) {
            lock (_lock) {
                if (_sleeping.Contains( thread )) {
                    _sleeping.Remove( thread );
                }
            }
        }

        private bool CheckStartedOrActive( int thread ) {
            lock (_lock) {
                return _started.Contains( thread ) || _active.Contains( thread );
            }
        }

        private bool CheckActive( int thread ) {
            lock (_lock) {
                return _active.Contains( thread );
            }
        }

        private bool CheckSleeping( int thread ) {
            lock (_lock) {
                return _sleeping.Contains( thread );
            }
        }

        #endregion private and locked.


        #region internal.

        internal void RemoveSleepingThread( int thread ) {
            _log?.LogDebug( "Thread#{int} - Start RemoveSleepingThread", thread );
            SetHighWaterMarks( );
            RemoveSleeping( thread );
            _log?.LogDebug( "Thread#{int} - Exit RemoveSleepingThread", thread );
        }

        internal void ThreadActive( int thread ) {
            _log?.LogDebug( "Thread#{int} - Start ThreadActive", thread );
            SetHighWaterMarks( );
            bool threadActive = CheckActive( thread );
            if (threadActive) {
                _log?.LogDebug( "Thread#{int} - Exit early ThreadActive", thread );
                return;
            }
            RemoveStarted( thread );
            RemoveSleeping( thread );
            AddActive( thread );
            _log?.LogDebug( "Thread#{int} - Exit ThreadActive", thread );
        }

        internal void StartThread( int thread, int partNumber ) {
            _log?.LogDebug( "Thread#{int} - Start StartThread - ParNumber: {int}", thread, partNumber );
            SetHighWaterMarks( );
            bool bailOut = CheckStartedOrActive( thread );
            if (bailOut) {
                _log?.LogDebug( "Thread#{int} - Exit early StartThread - ParNumber: {int}", thread, partNumber );
                return;
            }
            RemoveSleeping( thread );
            RemoveActive( thread );
            AddStartThread( thread );
            _log?.LogDebug( "Thread#{int} - Exit StartThread - ParNumber: {int}", thread, partNumber );
        }

        internal void ThreadSleeping( int thread ) {
            _log?.LogDebug( "Thread#{int} - Start ThreadSleeping", thread );
            SetHighWaterMarks( );
            RemoveStarted( thread );
            RemoveActive( thread );
            AddSleeping( thread );
            _log?.LogDebug( "Thread#{int} - Exit ThreadSleeping", thread );
        }

        internal void FailThread( int thread, int partNumber ) {
            _log?.LogDebug( "Thread#{int} - Start FailThread - ParNumber: {int}", thread, partNumber );
            SetHighWaterMarks( );
            RemoveStarted( thread );
            RemoveActive( thread );
            RemoveSleeping( thread );
            AddFailed( thread );
            _log?.LogDebug( "Thread#{int} - Exit FailThread - ParNumber: {int}", thread, partNumber );
        }

        internal void ThreadCompleted( int thread ) {
            _log?.LogDebug( "Thread#{int} - Start ThreadCompleted", thread );
            SetHighWaterMarks( );
            bool threadSleeping = CheckSleeping( thread );
            if (threadSleeping) {
                _log?.LogDebug( "Thread#{int} - Exit early ThreadCompleted", thread );
                return;
            }
            RemoveActive( thread );
            AddCompleted( thread );
            _log?.LogDebug( "Thread#{int} - Exit ThreadCompleted", thread );
        }

        #endregion internal.


        #endregion Methods
    }
}
