namespace Cloud_ShareSync.Core.Configuration.Enums {
    [Flags]
    public enum Cloud_ShareSync_Features {
        Log4Net = 2,
        Encryption = 4,
        Compression = 8,
        Sqlite = 16,
        BackBlazeB2 = 32,
        Backup = 64,
        Restore = 128,
        Postgres = 256,
        AzureBlobStorage = 512,
        AwsS3 = 1024,
        GoogleCloudStorage = 2048
    }
}
