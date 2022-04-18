using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Text;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private string FinishLargeFileRequestToJson( B2FinishLargeFileRequest finishLargeFileData ) {
            using Activity? activity = _source.StartActivity( "FinishLargeFileRequestToJson" )?.Start( );

            DataContractJsonSerializer? serializer = new( typeof( B2FinishLargeFileRequest ) );

            using MemoryStream? stream = new( );
            serializer.WriteObject( stream, finishLargeFileData );

            activity?.Stop( );
            return Encoding.UTF8.GetString( stream.ToArray( ) );
        }

    }
}
