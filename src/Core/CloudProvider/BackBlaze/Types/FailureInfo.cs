namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class FailureInfo {
        public int RetryWaitTimer { get; set; }
        public int? StatusCode { get; set; } = null;
        public DateTime? FailureTime { get; set; } = null;
        public DateTime? PastFailureTime { get; set; } = null;

        public FailureInfo( int? retryNum ) {
            RetryWaitTimer = retryNum ?? 0;
        }
    }
}
