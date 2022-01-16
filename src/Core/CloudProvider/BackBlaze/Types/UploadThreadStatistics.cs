using System.Text.Json;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    public class UploadThreadStatistics {
        public int Thread { get; set; }
        public int Success { get; set; } = 0;
        public int Failure { get; set; } = 0;
        public int FailurePercentage { get; set; } = 0;
        public decimal SleepTimerAverage { get; set; } = 0;
        public int[] SleepTimers { get; set; } = Array.Empty<int>( );
        public int CumulativeSuccess { get; set; } = 0;
        public int CumulativeFailure { get; set; } = 0;
        public int CumulativeFailurePercentage { get; set; } = 0;
        public int[] CumulativeSleepTimers { get; set; } = Array.Empty<int>( );
        public decimal CumulativeSleepTimerAverage { get; set; } = 0;

        public UploadThreadStatistics( int thread ) {
            Thread = thread;
        }

        internal void ResetSleepStats( ) {
            List<int> sleepTimers = new( );
            if (CumulativeSleepTimers.Length > 0) {
                sleepTimers.AddRange( CumulativeSleepTimers );
            }
            if (SleepTimers.Length > 0) {
                sleepTimers.AddRange( SleepTimers );
            }
            CumulativeSleepTimers = sleepTimers.ToArray( );

            CumulativeSuccess += Success;
            CumulativeFailure += Failure;

            CumulativeFailurePercentage = (CumulativeSuccess > 0) ?
                (CumulativeFailure / CumulativeSuccess) * 100 :
                100;

            Success = 0;
            Failure = 0;
            FailurePercentage = 0;
            SleepTimerAverage = 0;
            SleepTimers = Array.Empty<int>( );
        }

        internal void AddSleepTimer( int sleepSeconds ) {
            List<int> sleepTimers = new( );
            if (SleepTimers.Length > 0) {
                sleepTimers.AddRange( SleepTimers );
            }
            sleepTimers.Add( sleepSeconds );
            SleepTimers = sleepTimers.ToArray( );
        }

        internal void CalculateStats( ) {
            int sleepTotal = 0;
            foreach (int sleepTimer in SleepTimers) {
                sleepTotal += sleepTimer;
            }
            if (SleepTimers.Length > 0) {
                SleepTimerAverage = sleepTotal / SleepTimers.Length;
            }
            FailurePercentage = Success > 0 ? (Failure / Success) * 100 : (Failure > 0) ? 100 : 0;

            List<int> cumulativeSleepTimers = new( );
            if (CumulativeSleepTimers.Length > 0) {
                cumulativeSleepTimers.AddRange( CumulativeSleepTimers );
            }
            if (SleepTimers.Length > 0) {
                cumulativeSleepTimers.AddRange( SleepTimers );
            }

            int cumulativeSleepTotal = 0;
            foreach (int sleepTimer in cumulativeSleepTimers) {
                cumulativeSleepTotal += sleepTimer;
            }
            if (CumulativeSleepTimers.Length > 0) {
                CumulativeSleepTimerAverage = cumulativeSleepTotal / CumulativeSleepTimers.Length;
            }

            CumulativeFailurePercentage = (CumulativeSuccess > 0) ?
                (CumulativeFailure / CumulativeSuccess) * 100 :
                (CumulativeFailure > 0) ? 100 : 0;

        }

        public override string ToString( ) {
            CalculateStats( );
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
