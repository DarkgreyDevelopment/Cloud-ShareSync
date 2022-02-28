namespace Cloud_ShareSync.Core.Configuration.Enums {
    /// <summary>
    /// The list of supported cloud providers that can be optionally enabled.
    /// At least one of these is required and its configuration must be specified
    /// for the application to run.
    /// </summary>
    [Flags]
    public enum CloudProviders {
        /// <summary>
        /// Enables Cloud-ShareSync to upload and download to BackBlaze B2 cloud storage.
        /// </summary>
        BackBlazeB2 = 2
    }
}
