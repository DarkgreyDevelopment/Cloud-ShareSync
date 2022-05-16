using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums {
    [Flags]
    [JsonConverter( typeof( AllowedCapabilitiesJsonConverter ) )]
    internal enum AuthorizeAccountAllowedCapabilities {
        bypassGovernance,
        deleteBuckets,
        deleteFiles,
        deleteKeys,
        listBuckets,
        listFiles,
        listKeys,
        readBucketEncryption,
        readBucketRetentions,
        readBuckets,
        readFileLegalHolds,
        readFileRetentions,
        readFiles,
        shareFiles,
        writeBucketEncryption,
        writeBucketRetentions,
        writeBuckets,
        writeFileLegalHolds,
        writeFileRetentions,
        writeFiles,
        writeKeys,
        nullOption
    }
}
