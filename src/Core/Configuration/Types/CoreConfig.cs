using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Configuration.Enums;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// <para>
    /// The minimum configuration values required to start Cloud-ShareSync.
    /// </para>
    /// The enumerations provided are used to enable additional features within the application.
    /// </summary>
    public class CoreConfig {

        /// <summary>
        /// The list of features to enable and do configuration validation on.
        /// </summary>
        /// <value>
        /// Defaults:<br></br>
        /// <see cref="Cloud_ShareSync_Features.Log4Net"/><br></br>
        /// <see cref="Cloud_ShareSync_Features.Sqlite"/><br></br>
        /// <see cref="Cloud_ShareSync_Features.BackBlazeB2"/><br></br>
        /// <see cref="Cloud_ShareSync_Features.Backup"/><br></br>
        /// <see cref="Cloud_ShareSync_Features.Restore"/>
        /// </value>
        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public Cloud_ShareSync_Features EnabledFeatures { get; set; } =
            Cloud_ShareSync_Features.Log4Net |
            Cloud_ShareSync_Features.Sqlite |
            Cloud_ShareSync_Features.BackBlazeB2 |
            Cloud_ShareSync_Features.Backup |
            Cloud_ShareSync_Features.Restore;

        /// <summary>
        /// The list of cloud providers to use in the Cloud-ShareSync process.
        /// </summary>
        /// <value>
        /// Defaults:<br></br>
        /// <see cref="CloudProviders.BackBlazeB2"/>
        /// </value>
        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public CloudProviders EnabledCloudProviders { get; set; } = CloudProviders.BackBlazeB2;

        /// <value>Returns the <see cref="CoreConfig"/> as a json string.</value>
        public override string ToString( ) =>
            JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
    }
#nullable enable
}
