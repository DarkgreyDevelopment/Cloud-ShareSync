using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    internal class Retention_StartLargeFileAndCopyFile {
#pragma warning disable IDE1006 // Naming Styles - Names Match B2 fields
        public FileLockMode? mode;
        public long retainUntilTimestamp;
#pragma warning restore IDE1006 // Naming Styles - Names Match B2 fields
    }
}
