namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class B2ConcurrentStats {
        public B2ConcurrentStats( int available ) {
            _semaphore.Release(1);
            Available = available;
            _started = new( );
            _active = new( );
            _failed = new( );
            _sleeping = new( );
            _completed = new( );
        }

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

        private readonly SemaphoreSlim _semaphore = new( 0, 1 );

        public int HighWaterActive => Active >= HighWaterActive ? Active : HighWaterActive;
        public int HighWaterSleeping => Sleeping >= HighWaterSleeping ? Sleeping : HighWaterSleeping;

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

        private void RemoveStarted( int thread ) {
            _semaphore.Wait( );
            if (_started.Contains( thread )) {
                _started.Remove( thread );
            }
            _semaphore.Release( );
        }

        internal void RemoveSleeping( int thread ) {
            _semaphore.Wait( );
            if (_sleeping.Contains( thread )) {
                _sleeping.Remove( thread );
            }
            _semaphore.Release( );
        }

        private void RemoveActive( int thread ) {
            _semaphore.Wait( );
            if (_active.Contains( thread )) {
                _active.Remove( thread );
            }
            _semaphore.Release( );
        }

        internal void StartThread( int thread ) {
            _semaphore.Wait( );
            bool threadStarted = _started.Contains( thread ) || _active.Contains( thread );
            _semaphore.Release( );
            if (threadStarted) { return; }

            RemoveSleeping( thread );
            RemoveActive( thread );

            _semaphore.Wait( );
            _started.Add( thread );
            _semaphore.Release( );
        }

        internal void ThreadActive( int thread ) {
            _semaphore.Wait( );
            bool threadActive = _active.Contains( thread );
            _semaphore.Release( );
            if (threadActive) { return; }

            RemoveStarted( thread );
            RemoveSleeping( thread );

            _semaphore.Wait( );
            _active.Add( thread );
            _semaphore.Release( );
        }

        internal void ThreadSleeping( int thread ) {
            RemoveStarted( thread );
            RemoveActive( thread );
            _semaphore.Wait( );
            _sleeping.Add( thread );
            _semaphore.Release( );
        }

        internal void FailThread( int thread ) {
            RemoveStarted( thread );
            RemoveActive( thread );
            RemoveSleeping( thread );

            _semaphore.Wait( );
            _failed.Add( thread );
            _semaphore.Release( );
        }

        internal void ThreadCompleted( int thread ) {
            _semaphore.Wait( );
            bool threadSleeping = _sleeping.Contains( thread );
            _semaphore.Release( );
            if (threadSleeping) { return; }

            RemoveActive( thread );
            _semaphore.Wait( );
            _completed.Add( thread );
            _semaphore.Release( );
        }
    }
}
