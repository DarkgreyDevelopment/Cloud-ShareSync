namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions {
    internal class FailedB2RequestException : Exception {
        public FailedB2RequestException( ) : base( ) { }
        public FailedB2RequestException( string message ) : base( message ) { }
        public FailedB2RequestException( string message, Exception inner ) : base( message, inner ) { }
    }
}
