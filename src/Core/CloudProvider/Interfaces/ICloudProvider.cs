using Cloud_ShareSync.Core.Configuration.Interfaces;

namespace Cloud_ShareSync.Core.CloudProvider.Interfaces {
    internal interface ICloudProvider {
        void Initialize( ICloudProviderConfig config ) { }
        void UploadFile( ICloudProviderUpload upload ) { }
        void DownloadFile( ICloudProviderDownload download ) { }
    }
}
