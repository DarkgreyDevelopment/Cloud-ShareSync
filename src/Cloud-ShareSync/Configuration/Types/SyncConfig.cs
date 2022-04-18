using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Configuration.Enums;
using Cloud_ShareSync.Configuration.Interfaces;

namespace Cloud_ShareSync.Configuration.Types {
#nullable disable
    /// <summary>
    /// The primary configuration settings that drive Cloud-ShareSync.
    /// </summary>
    public class SyncConfig : ICloudShareSyncConfig {

        #region Constructors

        public SyncConfig( ) { }

        public SyncConfig( string syncFolder ) { SyncFolder = syncFolder; }

        #endregion Constructors


        #region Fields

        internal const string DefaultSyncFolder = "{SyncFolder}";


        /// <summary>
        /// The root directory to sync with configured cloud providers.
        /// </summary>
        public string SyncFolder { get; set; }


        /// <summary>
        /// When enabled Cloud-ShareSync will sync files in directories below the <see cref="SyncFolder"/>.<br/>
        /// When disabled Cloud-ShareSync will only sync files directly in the root of the <see cref="SyncFolder"/>.<br/>
        /// </summary>
        /// <value><see langword="true"/></value>
        public bool Recurse { get; set; } = true;


        /// <summary>
        /// Specify paths to exclude from the sync process.<br/>
        /// You may use <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference">regex</see> here.<br/>
        /// </summary>
        /// <value>An empty <see langword="string"/> <see langword="array"/>.</value>
        public string[] ExcludePaths { get; set; } = Array.Empty<string>( );


        /// <summary>
        /// Set to the path of the directory to use as a working directory when compressing or encrypting files,
        /// as well as for file decompression and decryption.<br/>
        /// This does not need to be set unless <see cref="EncryptBeforeUpload"/> or <see cref="CompressBeforeUpload"/>
        /// have been enabled.<br/>
        /// </summary>
        /// <value>An empty <see langword="string"/></value>
        public string WorkingDirectory { get; set; } = "";


        /// <summary>
        /// When enabled Cloud-ShareSync will use <see cref="ManagedCompression"/> to compress files prior 
        /// to uploading to all configured cloud providers.<br/>
        /// To enable this feature you must also add <see cref="Cloud_ShareSync_Features.Compression"/> to the 
        /// <see cref="EnabledFeatures"/>.<br/>
        /// When disabled files will not be compressed prior to upload. However if you attempt to download files that
        /// were previously compressed before upload they will still be decompressed automatically as long as
        /// <see cref="Cloud_ShareSync_Features.Compression"/> remains in the <see cref="EnabledFeatures"/>.<br/>
        /// </summary>
        /// <value><see langword="false"/></value>
        public bool CompressBeforeUpload { get; set; }


        /// <summary>
        /// When <see cref="CompressBeforeUpload"/> is enabled this field enables optional password protections of
        /// the compressed files.<br/>
        /// When <see cref="UniqueCompressionPasswords"/> is enabled <see cref="UniquePassword"/> will generate a random
        /// 100 character password for each compressed file.<br/>
        /// When disabled compressed files will not be password protected.<br/>
        /// </summary>
        /// <value><see langword="false"/></value>
        public bool UniqueCompressionPasswords { get; set; }


        /// <summary>
        /// When enabled Cloud-ShareSync will use <see cref="ManagedChaCha20Poly1305"/> to encrypt files
        /// prior to uploading to all configured cloud providers.<br/>
        /// To enable this feature you must also add <see cref="Cloud_ShareSync_Features.Encryption"/> to the 
        /// <see cref="EnabledFeatures"/>.<br/>
        /// When disabled files will not be encrypted prior to upload. However if you attempt to download files that
        /// were previously encrypted before upload they will still be decrypted automatically as long as
        /// <see cref="Cloud_ShareSync_Features.Encryption"/> remains in the <see cref="EnabledFeatures"/>.<br/>
        /// </summary>
        /// <value><see langword="false"/></value>
        public bool EncryptBeforeUpload { get; set; }


        /// <summary>
        /// When enabled files will be uploaded to configured cloud providers with their filename set
        /// to the sha512 hashed value of the pre-upload file hash. 
        /// <para>
        /// Example:<br/>
        /// A file named "ABC.txt" has a filehash with a hex encoded string of "0123456789".<br/>
        /// When this feature is enabled Cloud-ShareSync will call <see cref = "Hashing.GetSha512Hash(string)"/>
        /// on the string "0123456789".<br/>
        /// "ABC.txt" would then be uploaded with a filename of 
        /// "bb96c2fc40d2d54617d6f276febe571f623a8dadf0b734855299b0e107fda32cf6b69f2da32b36445d73690b93cbd0f7bfc20e0f7f28553d2a4428f23b716e90".<br/>
        /// When Cloud-ShareSync re-downloaded the file it would be correctly named "ABC.txt".
        /// </para>
        /// </summary>
        /// <value><see langword="false"/></value>
        public bool ObfuscateUploadedFileNames { get; set; }


        /// <summary>
        /// The list of features to enable and do configuration validation on.
        /// </summary>
        /// <value>
        /// <see cref="Cloud_ShareSync_Features.Log4Net"/><br/>
        /// <see cref="Cloud_ShareSync_Features.Sqlite"/><br/>
        /// <see cref="Cloud_ShareSync_Features.BackBlazeB2"/><br/>
        /// <see cref="Cloud_ShareSync_Features.Backup"/><br/>
        /// <see cref="Cloud_ShareSync_Features.Restore"/>
        /// </value>
        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public Cloud_ShareSync_Features EnabledFeatures { get; set; } =
            Cloud_ShareSync_Features.Log4Net |
            Cloud_ShareSync_Features.Sqlite |
            Cloud_ShareSync_Features.BackBlazeB2 |
            Cloud_ShareSync_Features.Backup |
            Cloud_ShareSync_Features.Restore; // 242


        /// <summary>
        /// The list of cloud providers to use in the Cloud-ShareSync process.
        /// </summary>
        /// <value><see cref="CloudProviders.BackBlazeB2"/></value>
        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public CloudProviders EnabledCloudProviders { get; set; } = CloudProviders.BackBlazeB2;

        #endregion Fields


        #region Methods

        /// <summary>
        /// Returns the <see cref="SyncConfig"/> as a json formatted string.
        /// </summary>
        public override string ToString( ) =>
            JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );

        #endregion Methods
    }
#nullable enable
}
