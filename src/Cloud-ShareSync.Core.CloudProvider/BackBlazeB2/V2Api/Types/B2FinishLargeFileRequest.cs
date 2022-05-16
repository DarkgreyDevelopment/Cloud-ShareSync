using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    [DataContract]
    internal class B2FinishLargeFileRequest {
        public B2FinishLargeFileRequest( ) { }

        public B2FinishLargeFileRequest(
            string fileId,
            List<KeyValuePair<int, string>> sha1Parts
        ) {
            this.fileId = fileId;
            partSha1Array = OrderSha1Parts( sha1Parts );
        }

#pragma warning disable IDE1006 // Naming Styles
#nullable disable
        [DataMember]
        internal string fileId;
        [DataMember]
        internal List<string> partSha1Array;
#pragma warning restore IDE1006 // Naming Styles
#nullable enable

        private static List<string> OrderSha1Parts( List<KeyValuePair<int, string>> sha1Parts ) =>
            sha1Parts.OrderBy( x => x.Key ).Select( x => x.Value ).ToList( );

        public override string ToString( ) {
            DataContractJsonSerializer? serializer = new( typeof( B2FinishLargeFileRequest ) );
            using MemoryStream stream = new( );
            serializer.WriteObject( stream, this );
            return Encoding.UTF8.GetString( stream.ToArray( ) );
        }
    }
}
