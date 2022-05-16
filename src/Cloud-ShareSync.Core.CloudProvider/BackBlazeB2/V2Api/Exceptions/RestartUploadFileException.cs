namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions {
    internal class RestartUploadFileException : Exception {
        public RestartUploadFileException( ) : base( ) { }
        public RestartUploadFileException( string message ) : base( message ) { }
        public RestartUploadFileException( string message, Exception inner ) : base( message, inner ) { }
    }
}
