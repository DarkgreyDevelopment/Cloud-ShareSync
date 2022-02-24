namespace Cloud_ShareSync.CloudProvider.BackBlaze.Types {
    internal class ThreadProcessTime {

        public readonly int ThreadNumber;

        public readonly List<PartProcessTime> PartTimes = new( );

        public ThreadProcessTime( int thread ) {
            ThreadNumber = thread;
        }
    }
}
