namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions {
    internal class RestartUploadPartException : Exception {
        public RestartUploadPartException( ) : base( ) { }
        public RestartUploadPartException( string message ) : base( message ) { }
        public RestartUploadPartException( string message, Exception inner ) : base( message, inner ) { }
    }
}
