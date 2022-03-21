using System.Text.Json;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading {
    internal class B2ThreadStatistic {
        public int Thread { get; private set; }
        public int Attempt { get; set; }
        public int Success { get; set; }
        public int Failure { get; set; }
        public decimal SuccessPercentage => CalculatePercentage( Success, Attempt );
        public decimal FailurePercentage => CalculatePercentage( Failure, Attempt );
        internal List<int> SleepTimers { get; } = new( );
        public int SleepTimerCount => SleepTimers.Count;
        public decimal SleepTimerAverage => CalculateSleepTimerAverage( SleepTimers );
        public decimal AverageSecondsAsleepPerSuccess => CalculateSleepSuccessAverage( SleepTimers, Success );


        // CTor
        public B2ThreadStatistic( int thread ) { Thread = thread; }

        public void AddSleepTimer( int sleepTimer ) {
            SleepTimers.Add( sleepTimer );
        }

        private static decimal CalculatePercentage( int numerator, int denominator ) {
            return denominator > 0 ? (decimal)numerator / denominator * 100 : 0;
        }

        private static decimal CalculateSleepSuccessAverage( List<int> intList, int successCount ) {
            decimal result = 0;

            if (intList.Count > 0 && successCount > 0) {
                int cumulativeSleepTotal = 0;
                foreach (int sleepTimer in intList) {
                    cumulativeSleepTotal += sleepTimer;
                }

                result = (decimal)cumulativeSleepTotal / successCount;
            }

            return result;
        }

        private static decimal CalculateSleepTimerAverage( List<int> intList ) {
            decimal result = 0;

            if (intList.Count > 0) {
                int cumulativeSleepTotal = 0;
                foreach (int sleepTimer in intList) {
                    cumulativeSleepTotal += sleepTimer;
                }

                result = (decimal)cumulativeSleepTotal / intList.Count;
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
