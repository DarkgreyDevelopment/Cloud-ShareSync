namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class FailureInfo {
        public int RetryWaitTimer { get; set; }
        public int? StatusCode { get; set; } = null;
        public DateTime? FailureTime { get; set; } = null;
        public DateTime? PastFailureTime { get; set; } = null;

        private static readonly Random s_random = new( );
        private static readonly int[] s_retryStartSec = new int[] { 0, 1, 3, 7 };

        public FailureInfo( ) {
            RetryWaitTimer = s_retryStartSec[s_random.Next( 0, s_retryStartSec.Length )];
        }

        public void Reset( ) {
            RetryWaitTimer = s_retryStartSec[s_random.Next( 0, s_retryStartSec.Length )];
            StatusCode = null;
            FailureTime = null;
            PastFailureTime = null;
        }
    }
}
