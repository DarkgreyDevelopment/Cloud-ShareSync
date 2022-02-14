using Cloud_ShareSync.Core.CloudProvider.Interface;
using Cloud_ShareSync.Core.Configuration.Interfaces;

namespace Cloud_ShareSync.Core.CloudProvider.Azure {
    public class AzureBlobStorage : ICloudProvider {
        public static void Initialize( ICloudProviderConfig config ) { throw new NotImplementedException( ); }
        public static void UploadFile( ICloudProviderUpload upload ) { throw new NotImplementedException( ); }
        public static void DownloadFile( ICloudProviderDownload download ) { throw new NotImplementedException( ); }
    }
}
