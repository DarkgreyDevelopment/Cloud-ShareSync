namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions {
    internal class NewAuthTokenRequiredException : Exception {
        public NewAuthTokenRequiredException( ) : base( ) { }
        public NewAuthTokenRequiredException( string message ) : base( message ) { }
        public NewAuthTokenRequiredException( string message, Exception inner ) : base( message, inner ) { }
    }
}
