namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    internal class UploadResultInfo {
        public UploadResultInfo( FilePartResult result ) {
            Result = result;
        }

        public FilePartResult Result { get; private set; }
        public FilePartInfo? Info { get; set; }

    }
}
