namespace Cloud_ShareSync.Core.CloudProvider.Interface {
    public interface ICloudProvider {
        void Initialize( ICloudProviderConfig config ) { }
        void UploadFile( ICloudProviderUpload upload ) { }
        void DownloadFile( ICloudProviderDownload download ) { }
    }
}
