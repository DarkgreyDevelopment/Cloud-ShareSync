using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    internal class AuthorizeAccountAllowed {
#pragma warning disable IDE1006 // Naming Styles - Names Match B2 fields
        public string? bucketId { get; set; }
        public string? bucketName { get; set; }
        public AuthorizeAccountAllowedCapabilities capabilities { get; set; }
        public string? namePrefix { get; set; }
#pragma warning restore IDE1006 // Naming Styles - Names Match B2 fields
    }
}
