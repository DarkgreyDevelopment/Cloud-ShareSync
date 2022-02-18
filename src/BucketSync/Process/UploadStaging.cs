using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.BucketSync.Process {
    public class UploadStaging {
        //public PrimaryTable PrimaryData { get; set; }
        public BackBlazeB2Table BackBlazeData { get; set; }
        public EncryptionTable? EncryptionData { get; set; }
        public CompressionTable? CompressionData { get; set; }

        public UploadStaging(
            //PrimaryTable primaryData,
            BackBlazeB2Table backBlazeData
        ) {
            //PrimaryData = primaryData;
            BackBlazeData = backBlazeData;
        }
    }
}
