using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    public class B2File {
#pragma warning disable IDE1006 // Naming Styles - Names Match B2 fields
        public string accountId { get; set; } = string.Empty;
        public ResponseAction action { get; set; }
        public string bucketId { get; set; } = string.Empty;
        public long contentLength { get; set; }
        public string? contentMd5 { get; set; }
        public string contentSha1 { get; set; } = string.Empty;
        public string? contentType { get; set; }
        public string? fileId { get; set; }
        public Dictionary<string, string> fileInfo { get; set; } = new( );
        public string fileName { get; set; } = string.Empty;
        //public Retention_StartLargeFileAndCopyFile? fileRetention { get; set; }
        //public string legalHold { get; set; } = string.Empty;
        public CloudReplicationStatus? replicationStatus { get; set; }
        //public ServerSideEncryptionState serverSideEncryption { get; set; } = new( );
        public long uploadTimestamp { get; set; }
#pragma warning restore IDE1006 // Naming Styles - Names Match B2 fields
        public DateTime UploadDateTime => DateTimeOffset.FromUnixTimeMilliseconds( uploadTimestamp ).DateTime;
    }
}
