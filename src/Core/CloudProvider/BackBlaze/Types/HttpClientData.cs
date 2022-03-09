using System.Net.Http.Headers;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class HttpClientData {
        internal HttpClient Client { get; } = new( );
        internal DateTime InitTime { get; } = DateTime.Now;
        internal DateTime? LastUse { get; set; } = null;
        internal bool Dispose { get; set; } = false;

        internal HttpClientData( ) {
            Client
            .DefaultRequestHeaders
            .Accept
            .Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
        }
    }
}
