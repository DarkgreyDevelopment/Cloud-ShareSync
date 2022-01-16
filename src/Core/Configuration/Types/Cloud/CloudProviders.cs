namespace Cloud_ShareSync.Core.Configuration.Types.Cloud {
    [Flags]
    public enum CloudProviders {
        AwsS3 = 2,
        AzureBlobStorage = 4,
        BackBlazeB2 = 8,
        GoogleCloudStorage = 16
    }
}
