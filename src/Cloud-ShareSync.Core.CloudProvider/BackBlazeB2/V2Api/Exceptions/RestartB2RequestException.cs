namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions {
    internal class RestartB2RequestException : Exception {
        public RestartB2RequestException( ) : base( ) { }
        public RestartB2RequestException( string message ) : base( message ) { }
        public RestartB2RequestException( string message, Exception inner ) : base( message, inner ) { }
    }
}
