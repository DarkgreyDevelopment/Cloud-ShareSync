namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    internal class DownloadResultInfo {
        public DownloadResultInfo( byte[] data, long partNumber ) {
            Data = data;
            PartNumber = partNumber;
        }

        public byte[] Data { get; private set; }
        public long PartNumber { get; private set; }

    }
}
