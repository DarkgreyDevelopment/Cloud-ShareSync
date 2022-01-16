using Cloud_ShareSync.Core.CloudProvider.Interface;

namespace Cloud_ShareSync.Core.CloudProvider.Azure {
    public class AzureBlobStorage : ICloudProvider {
        public static void Initialize( ICloudProviderConfig config ) { throw new NotImplementedException( ); }
        public static void UploadFile( ICloudProviderUpload upload ) { throw new NotImplementedException( ); }
        public static void DownloadFile( ICloudProviderDownload download ) { throw new NotImplementedException( ); }
    }
}
