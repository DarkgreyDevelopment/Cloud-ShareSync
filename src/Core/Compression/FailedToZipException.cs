namespace Cloud_ShareSync.Core.Compression {
    [Serializable]
    internal class FailedToZipException : Exception {
        public FailedToZipException( ) : base( ) { }
        public FailedToZipException( string message ) : base( message ) { }
        public FailedToZipException( string message, Exception inner ) : base( message, inner ) { }

        protected FailedToZipException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
        ) : base( info, context ) { }
    }
}
