namespace Cloud_ShareSync.Core.CloudProvider.SharedServices {

    public class CloudShareSyncHttpClient {

        public HttpClient HttpClient { get; }

        public CloudShareSyncHttpClient( HttpClient client ) { HttpClient = client; }

    }
}
