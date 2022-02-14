﻿namespace Cloud_ShareSync.Core.Configuration.Enums {
    [Flags]
    public enum Cloud_ShareSync_Features {
        Log4Net = 1,
        Encryption = 2,
        Compression = 4,
        AwsS3 = 8,
        AzureBlobStorage = 16,
        BackBlazeB2 = 32,
        GoogleCloudStorage = 64,
        SimpleBackup = 128,
        SimpleRestore = 256,
        Sqlite = 512,
        Postgres = 1024
    }
}