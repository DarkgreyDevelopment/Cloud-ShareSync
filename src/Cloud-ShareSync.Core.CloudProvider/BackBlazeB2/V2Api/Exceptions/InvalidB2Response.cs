namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions {
    [Serializable]
    internal class InvalidB2Response : Exception {
        private const string DefaultMsg = "Received an invalid response from Backblaze. Api uri: ";

        internal InvalidB2Response( ) : base( ) { }
        internal InvalidB2Response( string apiUri ) : base( DefaultMsg + apiUri ) { }
        internal InvalidB2Response( string apiUri, Exception inner ) : base( DefaultMsg + apiUri, inner ) { }

        protected InvalidB2Response( System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
    }
}
