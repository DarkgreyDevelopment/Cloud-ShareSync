﻿using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal static class B2ThreadManager {
        public const int MinimumThreadCount = 1;
        public static int MaximumThreadCount = 1;
        public static UploadThreadStatistic[] ThreadStats = Array.Empty<UploadThreadStatistic>( );
        public static FailureInfo[] FailureDetails = Array.Empty<FailureInfo>( );
        public static List<B2ConcurrentStats> ConcurrencyStats = new( );

        private static ILogger? s_log;

        private static readonly object s_statsLock = new( );
        private static int s_activeThreadCount = 0;
        private static int s_lastChange = 0;
        private static decimal s_previousAverageSuccessPercentage = 0;
        private static decimal s_previousSecondsAsleepPerSuccess = 0;
        private static decimal s_previousAverageSleepTimerLength = 0;
        private static decimal s_previousAverageHighWaterSleeping = 0;
        private static int s_previousFailedThreads = 0;

        public static void Inititalize( ILogger? log, int maxThreads ) {
            s_log = log;
            UpdateMaxThreadCount( maxThreads );
            s_activeThreadCount = MaximumThreadCount;
        }

        public static void UpdateMaxThreadCount( int maxThreads ) {

            MaximumThreadCount = maxThreads > 0 ? maxThreads : 1;
            List<UploadThreadStatistic> threadStats = new( );
            List<FailureInfo> failureDetails = new( );

            lock (s_statsLock) {
                int startThreadNum = ThreadStats.Length;
                if (ThreadStats.Length > 0) {
                    // If we already have items then we want to start at the next available thread number.
                    startThreadNum += 1;
                    threadStats.AddRange( ThreadStats );
                    failureDetails.AddRange( FailureDetails );
                }
                for (int i = startThreadNum; i < MaximumThreadCount; i++) {
                    threadStats.Add( new( i ) );
                    failureDetails.Add( new( ) );
                }
                FailureDetails = failureDetails.ToArray( );
                ThreadStats = threadStats.ToArray( );
            }
        }

        public static int GetActiveThreadCount( int partsTotal ) {
            AssessActiveThreadCount( );
            return s_activeThreadCount > partsTotal ? partsTotal : s_activeThreadCount;
        }

        private static void AssessActiveThreadCount( ) {

            int threadChange = 0;
            // Gather latest Upload Thread Info:
            IEnumerable<UploadThreadStatistic> stats = ThreadStats.Where( e => e.Attempt > 0 );
            long totalAttempts = stats.Select( e => e.Attempt ).Sum( );
            long totalSuccesses = stats.Select( e => e.Success ).Sum( );
            decimal averageSuccessPercentage = totalAttempts > 0 ? totalSuccesses / totalAttempts * 100 : 0;
            IEnumerable<int> sleeptimers = stats.SelectMany( e => e.SleepTimers );
            long sleepTimerTotal = sleeptimers.Sum( );
            long sleepTimerCount = sleeptimers.LongCount( );
            decimal secondsAsleepPerSuccess = totalSuccesses > 0 ? sleepTimerTotal / totalSuccesses : 0;
            long averageSleepTimerLength = sleepTimerCount > 0 ? sleepTimerTotal / sleepTimerCount : 0;

            // Gather latest concurrent stat info:
            IEnumerable<B2ConcurrentStats> cStats = ConcurrencyStats.TakeLast( ConcurrencyStats.Count >= 5 ? 5 : ConcurrencyStats.Count );
            int latestHighWaterSleeping = cStats.Select( e => e.HighWaterSleeping ).Sum( );
            int cStatsCount = cStats.Count( );
            decimal latestAverageHighWaterSleeping = cStatsCount > 0 ? latestHighWaterSleeping / cStatsCount : 0;
            int latestFailedThreads = cStats.Select( e => e.Failed ).Sum( );

            lock (s_statsLock) {
                bool changedThreadValue = false;

                // A change has been made recently. Allow time for stats to change.
                if (s_lastChange != 0) {
                    s_lastChange = threadChange;
                    return;
                }

                if (s_activeThreadCount > MinimumThreadCount) {
                    if (
                        latestFailedThreads > s_previousFailedThreads &&
                        changedThreadValue == false
                    ) {
                        s_log?.LogDebug( "There number of threads hitting max errors is increasing. Decreasing the number of available threads by 1." );
                        threadChange = -1;
                        changedThreadValue = true;
                    }

                    if (
                        latestAverageHighWaterSleeping > s_previousAverageHighWaterSleeping &&
                        changedThreadValue == false
                    ) {
                        s_log?.LogDebug( "More threads are sleeping on average. Decreasing the number of available threads by 1." );
                        threadChange = -1;
                        changedThreadValue = true;
                    }

                    // If the sleep timer average is trending up then reduce the number of threads.
                    if (
                        averageSleepTimerLength > s_previousAverageSleepTimerLength &&
                        changedThreadValue == false
                    ) {
                        s_log?.LogDebug( "The average sleep timer length is trending upwards. Decreasing the number of available threads by 1." );
                        threadChange = -1;
                        changedThreadValue = true;
                    }
                }

                if (s_activeThreadCount < MaximumThreadCount) {
                    // If success percentage is improving then allow more threads to work.
                    if (
                        averageSuccessPercentage > s_previousAverageSuccessPercentage &&
                        changedThreadValue == false
                    ) {
                        s_log?.LogDebug( "The success percentage is improving. Increasing the number of available threads by 1." );
                        threadChange = 1;
                        changedThreadValue = true;
                    }

                    if (
                        secondsAsleepPerSuccess < s_previousSecondsAsleepPerSuccess &&
                        changedThreadValue == false
                    ) {
                        s_log?.LogDebug( "Time spent asleep per success is trending downwards. Increasing the number of available threads by 1." );
                        threadChange = 1;
                        changedThreadValue = true;
                    }
                }

                s_previousAverageSuccessPercentage = averageSuccessPercentage;
                s_previousSecondsAsleepPerSuccess = secondsAsleepPerSuccess;
                s_previousAverageSleepTimerLength = averageSleepTimerLength;
                s_previousAverageHighWaterSleeping = latestAverageHighWaterSleeping;
                s_previousFailedThreads = latestFailedThreads;

                s_activeThreadCount += threadChange;
                s_lastChange = threadChange;
            }
        }

        public static void ShowThreadStatistics( bool? formatTable = null ) {

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

                s_log?.LogInformation(
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
                    s_log?.LogInformation( "{string}", msg );
                }
            } else {
                string stats = "{\n  \"ThreadStats\": [\n";
                foreach (UploadThreadStatistic stat in ThreadStats) {
                    stats += $"{stat},\n";
                }
                stats = stats.TrimEnd( '\n' ).TrimEnd( ',' );
                stats += "\n  ]\n}";
                s_log?.LogInformation( "{string}", stats );
            }
        }
    }
}
