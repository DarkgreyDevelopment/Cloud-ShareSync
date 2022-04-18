namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading {
    internal class ThreadProcessTime {

        public readonly int ThreadNumber;

        public readonly List<PartProcessTime> PartTimes = new( );

        public ThreadProcessTime( int thread ) {
            ThreadNumber = thread;
        }
    }
}
