using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class B2ThreadManager {
        public int ActiveThreadCount { get; private set; }
        public const int MinimumThreadCount = 1;
        public readonly int MaximumThreadCount;
        public readonly UploadThreadStatistic[] ThreadStats;
        public readonly FailureInfo[] FailureDetails;
        public readonly List<B2ConcurrentStats> ConcurrencyStats;

        private readonly ILogger? _log;

        public B2ThreadManager( ILogger? log, int maxThreads ) {
            _log = log;
            MaximumThreadCount = maxThreads > 0 ? maxThreads : 1;
            ActiveThreadCount = MaximumThreadCount;

            List<UploadThreadStatistic> threadStats = new( );
            List<FailureInfo> failureDetails = new( );
            for (int i = 0; i < MaximumThreadCount; i++) {
                threadStats.Add( new( i ) );
                failureDetails.Add( new( ) );
            }
            FailureDetails = failureDetails.ToArray( );
            ThreadStats = threadStats.ToArray( );
            ConcurrencyStats = new( );
        }

        public void ShowThreadStatistics( bool? formatTable = null ) {

            if (formatTable == true) {
                int threadColumnLength = 7;
                int attemptsColumnLength = 9;
                int successColumnLength = 8;
                int successPercentageColumnLength = 9;
                int failureColumnLength = 8;
                int failurePercentageColumnLength = 9;
                int sleepTimerCountColumnLength = 16;
                int averageSleepTimerColumnLength = 24;
                int secondsAsleepPerSuccessColumnLength = 24;

                int threadStringLength = 0;
                int attemptStringLength = 0;
                int successStringLength = 0;
                int failureStringLength = 0;
                int sleepTimerCountStringLength = 0;

                string decFormat = "000.000";

                // Loop through stats and determine string format max length for each prop.
                foreach (UploadThreadStatistic stat in ThreadStats) {
                    int tl = stat.Thread.ToString( ).Length;
                    int al = stat.Attempt.ToString( ).Length;
                    int sl = stat.Success.ToString( ).Length;
                    int fl = stat.Failure.ToString( ).Length;
                    int stcl = stat.SleepTimerCount.ToString( ).Length;
                    threadStringLength = tl > threadStringLength ? tl : threadStringLength;
                    attemptStringLength = al > attemptStringLength ? al : attemptStringLength;
                    successStringLength = sl > successStringLength ? sl : successStringLength;
                    failureStringLength = fl > failureStringLength ? fl : failureStringLength;
                    sleepTimerCountStringLength = stcl > sleepTimerCountStringLength ? stcl : sleepTimerCountStringLength;
                }

                string threadPad = threadColumnLength - threadStringLength > 0 ?
                    new( ' ', threadColumnLength - threadStringLength ) : "";
                string attemptPad = attemptsColumnLength - attemptStringLength > 0 ?
                    new( ' ', attemptsColumnLength - attemptStringLength ) : "";
                string successPad = successColumnLength - successStringLength > 0 ?
                    new( ' ', successColumnLength - successStringLength ) : "";
                string failurePad = failureColumnLength - failureStringLength > 0 ?
                    new( ' ', failureColumnLength - failureStringLength ) : "";
                string sleepTimerCountPad = sleepTimerCountColumnLength - sleepTimerCountStringLength > 0 ?
                    new( ' ', sleepTimerCountColumnLength - sleepTimerCountStringLength ) : "";

                _log?.LogDebug(
                    "| Thread | Attempts | Success | Success% | Failure | Failure%" +
                    " | SleepTimerCount | AverageSleepTimerLength | SecondsAsleepPerSuccess"
                );
                foreach (UploadThreadStatistic stat in ThreadStats) {
                    string secsAsleep = stat.AverageSecondsAsleepPerSuccess.ToString( "####0.0##" );
                    string msg = "| " +
                        stat.Thread.ToString( $"D{threadStringLength}" ) + threadPad + "| " +
                        stat.Attempt.ToString( $"D{attemptStringLength}" ) + attemptPad + "| " +
                        stat.Success.ToString( $"D{successStringLength}" ) + successPad + "| " +
                        stat.SuccessPercentage.ToString( decFormat ) + new string( ' ', successPercentageColumnLength - decFormat.Length ) + "| " +
                        stat.Failure.ToString( $"D{failureStringLength}" ) + failurePad + "| " +
                        stat.FailurePercentage.ToString( decFormat ) + new string( ' ', failurePercentageColumnLength - decFormat.Length ) + "| " +
                        stat.SleepTimerCount.ToString( $"D{sleepTimerCountStringLength}" ) + sleepTimerCountPad + "| " +
                        stat.SleepTimerAverage.ToString( decFormat ) + new string( ' ', averageSleepTimerColumnLength - decFormat.Length ) + "| " +
                        secsAsleep + new string( ' ', secondsAsleepPerSuccessColumnLength - secsAsleep.Length );
                    _log?.LogDebug( "{string}", msg );
                }
            } else {
                string stats = "{\n  \"ThreadStats\": [\n";
                foreach (UploadThreadStatistic stat in ThreadStats) {
                    stats += $"{stat},\n";
                }
                stats = stats.TrimEnd( '\n' ).TrimEnd( ',' );
                stats += "\n  ]\n}";
                _log?.LogDebug( "{string}", stats );
            }
        }
    }
}
