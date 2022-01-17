using System.Text.Json;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    public class UploadThreadStatistics {
        public int Thread { get; set; }
        public int Attempt { get; set; } = 0;
        public int CumulativeAttempts { get; set; } = 0;
        public int Success { get; set; } = 0;
        public int CumulativeSuccesses { get; set; } = 0;
        public int Failure { get; set; } = 0;
        public int CumulativeFailures { get; set; } = 0;
        public decimal SuccessPercentage { get; set; } = 0;
        public decimal CumulativeSuccessPercentage { get; set; } = 0;
        public decimal FailurePercentage { get; set; } = 0;
        public decimal CumulativeFailurePercentage { get; set; } = 0;
        public decimal SleepTimerAverage { get; set; } = 0;
        public decimal CumulativeSleepTimerAverage { get; set; } = 0;
        public decimal AverageTimeAsleepPerSuccess { get; set; } = 0;
        public int[] SleepTimers { get; set; } = Array.Empty<int>( );
        public int[] CumulativeSleepTimers { get; set; } = Array.Empty<int>( );

        public UploadThreadStatistics( int thread ) {
            Thread = thread;
        }

        internal void ResetThreadStats( ) {
            List<int> sleepTimers = new( );
            if (CumulativeSleepTimers.Length > 0) {
                sleepTimers.AddRange( CumulativeSleepTimers );
            }
            if (SleepTimers.Length > 0) {
                sleepTimers.AddRange( SleepTimers );
            }
            CumulativeSleepTimers = sleepTimers.ToArray( );

            CumulativeAttempts += Attempt;
            CumulativeSuccesses += Success;
            CumulativeFailures += Failure;

            CumulativeFailurePercentage = CumulativeAttempts > 0 ? ((decimal)CumulativeFailures / CumulativeAttempts) * 100 : 0;
            CumulativeSuccessPercentage = CumulativeAttempts > 0 ? ((decimal)CumulativeSuccesses / CumulativeAttempts) * 100 : 0;
            CumulativeSleepTimerAverage = (CumulativeSleepTimers.Length > 0) ?
                CalculateSleepTimerAverage( CumulativeSleepTimers ) : 0;
            AverageTimeAsleepPerSuccess = CumulativeSuccesses > 0 ?
                CumulativeSleepTimerAverage / CumulativeSuccesses : CumulativeSleepTimerAverage;

            Attempt = 0;
            Success = 0;
            Failure = 0;
            SuccessPercentage = 0;
            FailurePercentage = 0;
            SleepTimerAverage = 0;
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

        internal void AddSleepTimer( int sleepSeconds ) {
            List<int> sleepTimers = new( );
            if (SleepTimers.Length > 0) {
                sleepTimers.AddRange( SleepTimers );
            }
            sleepTimers.Add( sleepSeconds );
            SleepTimers = sleepTimers.ToArray( );
        }

        internal void CalculateStats( ) {

            int cumulativeAttempts = (Attempt + CumulativeAttempts);
            decimal cumulativeSuccess = Success + CumulativeSuccesses;
            // Calculate failure percentage.
            FailurePercentage = Attempt > 0 ? ((decimal)Failure / Attempt) * 100 : 0;
            CumulativeFailurePercentage = cumulativeAttempts > 0 ?
                ((decimal)(Failure + CumulativeFailures) / cumulativeAttempts) * 100 : 0;

            // Calculate success percentage.
            SuccessPercentage = Attempt > 0 ? ((decimal)Success / Attempt) * 100 : 0;
            CumulativeSuccessPercentage = cumulativeAttempts > 0 ?
                (cumulativeSuccess / cumulativeAttempts) * 100 : 0;

            // Calculate Sleep Timer Averages
            SleepTimerAverage = CalculateSleepTimerAverage( SleepTimers );

            // Calculate Cumulative Sleep Timer Average by
            // Adding current sleep timers to cumulative sleep timers.
            List<int> cumulativeSleepTimers = new( );
            if (CumulativeSleepTimers.Length > 0) {
                cumulativeSleepTimers.AddRange( CumulativeSleepTimers );
            }
            if (SleepTimers.Length > 0) {
                cumulativeSleepTimers.AddRange( SleepTimers );
            }
            CumulativeSleepTimerAverage = CalculateSleepTimerAverage( cumulativeSleepTimers.ToArray( ) );

            AverageTimeAsleepPerSuccess = cumulativeSuccess > 0 ?
                CumulativeSleepTimerAverage / cumulativeSuccess : CumulativeSleepTimerAverage;
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
