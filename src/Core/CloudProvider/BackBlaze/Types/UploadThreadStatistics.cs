using System.Text.Json;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    public class UploadThreadStatistics {
        public int Thread { get; set; }
        public int Attempt { get; private set; } = 0;
        public int CumulativeAttempts { get; private set; } = 0;
        public int Success { get; private set; } = 0;
        public int CumulativeSuccesses { get; private set; } = 0;
        public int Failure { get; private set; } = 0;
        public int CumulativeFailures { get; private set; } = 0;
        public int[] SleepTimers { get; private set; } = Array.Empty<int>( );
        public int[] CumulativeSleepTimers { get; private set; } = Array.Empty<int>( );
        public decimal SuccessPercentage => Attempt > 0 ? (decimal)Success / Attempt * 100 : 0;
        public decimal CumulativeSuccessPercentage => CumulativeAttempts > 0 ? CumulativeSuccesses / CumulativeAttempts * 100 : 0;
        public decimal FailurePercentage => Attempt > 0 ? (decimal)Failure / Attempt * 100 : 0;
        public decimal CumulativeFailurePercentage => CumulativeAttempts > 0 ? (decimal)(Failure + CumulativeFailures) / CumulativeAttempts * 100 : 0;
        public decimal SleepTimerAverage => CalculateSleepTimerAverage( SleepTimers );
        public decimal CumulativeSleepTimerAverage => CalculateSleepTimerAverage( CumulativeSleepTimers );
        public decimal AverageSecondsAsleepPerSuccess => CumulativeSuccesses > 0 ? CumulativeSleepTimerAverage / CumulativeSuccesses : CumulativeSleepTimerAverage;

        public UploadThreadStatistics( int thread ) {
            Thread = thread;
        }

        internal void NewAttempt( ) {
            Attempt++;
            CumulativeAttempts++;
        }

        internal void NewSuccess( ) {
            Success++;
            CumulativeSuccesses++;
        }

        internal void NewFailure( ) {
            Failure++;
            CumulativeFailures++;
        }

        internal void AddSleepTimer( int sleepSeconds ) {
            List<int> sleepTimers = new( );
            if (SleepTimers.Length > 0) {
                sleepTimers.AddRange( SleepTimers );
            }
            sleepTimers.Add( sleepSeconds );
            SleepTimers = sleepTimers.ToArray( );

            List<int> cumulativeSleepTimers = new( );
            if (CumulativeSleepTimers.Length > 0) {
                cumulativeSleepTimers.AddRange( CumulativeSleepTimers );
            }
            cumulativeSleepTimers.Add( sleepSeconds );
            CumulativeSleepTimers = cumulativeSleepTimers.ToArray( );
        }

        internal void ResetThreadStats( ) {
            Attempt = 0;
            Success = 0;
            Failure = 0;
            SleepTimers = Array.Empty<int>( );
        }

        private static decimal CalculateSleepTimerAverage( int[] intArray ) {
            decimal result = 0;

            if (intArray.Length > 0) {
                int cumulativeSleepTotal = 0;
                foreach (int sleepTimer in intArray) {
                    cumulativeSleepTotal += sleepTimer;
                }

                result = (decimal)cumulativeSleepTotal / intArray.Length;
            }

            return result;
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
