using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.Interfaces;
using Cloud_ShareSync.Core.Cryptography.FileEncryption;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// The primary configuration settings that drive Cloud-ShareSync.
    /// </summary>
    public class SyncConfig : ICloudShareSyncConfig {

        internal const string DefaultSyncFolder = "{SyncFolder}";

        #region Ctor

        public SyncConfig( ) { }

        public SyncConfig( string syncFolder ) { SyncFolder = syncFolder; }

        #endregion Ctor

        #region SyncFolder

        /// <summary>
        /// The root directory to sync with configured cloud providers.
        /// </summary>
        public string SyncFolder { get; set; }

        private static Option<DirectoryInfo> NewSyncFolderOption( Command verbCommand ) {
            Option<DirectoryInfo> syncFolderOption = new(
                name: "--SyncFolder",
                description: "Specify the path to the root directory to sync with configured cloud providers."
            );
            syncFolderOption.AddAlias( "-sf" );
            syncFolderOption.IsRequired = true;

            verbCommand.AddOption( syncFolderOption );

            return syncFolderOption;
        }

        #endregion SyncFolder


        #region Recurse

        /// <summary>
        /// When enabled Cloud-ShareSync will sync files in directories below the <see cref="SyncFolder"/>.<br/>
        /// When disabled Cloud-ShareSync will only sync files directly in the root of the <see cref="SyncFolder"/>.<br/>
        /// </summary>
        /// <value><see langword="true"/></value>
        public bool Recurse { get; set; } = true;

        private static Option<bool> NewRecurseOption( Command verbCommand ) {
            Option<bool> recurseOption = new(
                name: "--Recurse",
                description: "Enable to sync files in directories below the SyncFolder.",
                getDefaultValue: ( ) => true
            );
            recurseOption.AddAlias( "-r" );

            verbCommand.AddOption( recurseOption );

            return recurseOption;
        }

        #endregion Recurse


        #region ExcludePaths

        /// <summary>
        /// Specify paths to exclude from the sync process.<br/>
        /// You may use <see href="https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference">regex</see> here.<br/>
        /// </summary>
        /// <value>An empty <see langword="string"/> <see langword="array"/>.</value>
        public string[] ExcludePaths { get; set; } = Array.Empty<string>( );

        private static Option<string[]> NewExcludePathsOption( Command verbCommand ) {
            Option<string[]> excludePathsOption = new(
                name: "--ExcludePaths",
                description: "Specify paths to exclude from the sync process.",
                getDefaultValue: ( ) => Array.Empty<string>( )
            );
            excludePathsOption.AddAlias( "-ep" );
            excludePathsOption.AllowMultipleArgumentsPerToken = true;

            verbCommand.AddOption( excludePathsOption );

            return excludePathsOption;
        }

        #endregion ExcludePaths


        #region WorkingDirectory

        /// <summary>
        /// Set to the path of the directory to use as a working directory when compressing or encrypting files,
        /// as well as for file decompression and decryption.<br/>
        /// This does not need to be set unless <see cref="EncryptBeforeUpload"/> or <see cref="CompressBeforeUpload"/>
        /// have been enabled.<br/>
        /// </summary>
        /// <value>An empty <see langword="string"/></value>
        public string WorkingDirectory { get; set; } = "";

        private static Option<string> NewWorkingDirectoryOption( Command verbCommand ) {
            Option<string> workingDirectoryOption = new(
                name: "--WorkingDirectory",
                description: "Specify the path to a directory to use as a working directory " +
                "when compressing or encrypting files, as well as for file decompression and decryption.",
                getDefaultValue: ( ) => string.Empty
            );
            workingDirectoryOption.AddAlias( "-w" );

            verbCommand.AddOption( workingDirectoryOption );

            return workingDirectoryOption;
        }

        #endregion WorkingDirectory


        #region EncryptBeforeUpload

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

        private static Option<bool> NewEncryptBeforeUploadOption( Command verbCommand ) {
            Option<bool> encryptOption = new(
                name: "--EncryptBeforeUpload",
                description: "Enable to encrypt files prior to uploading to all configured cloud providers.",
                getDefaultValue: ( ) => false
            );
            encryptOption.AddAlias( "-eu" );

            verbCommand.AddOption( encryptOption );

            return encryptOption;
        }

        #endregion EncryptBeforeUpload


        #region CompressBeforeUpload

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

        private static Option<bool> NewCompressBeforeUploadOption( Command verbCommand ) {
            Option<bool> compressOption = new(
                name: "--CompressBeforeUpload",
                description: "Enable to compress files prior to uploading to all configured cloud providers.",
                getDefaultValue: ( ) => false
            );
            compressOption.AddAlias( "-c" );

            verbCommand.AddOption( compressOption );

            return compressOption;
        }

        #endregion CompressBeforeUpload


        #region UniqueCompressionPasswords

        /// <summary>
        /// When <see cref="CompressBeforeUpload"/> is enabled this field enables optional password protections of
        /// the compressed files.<br/>
        /// When <see cref="UniqueCompressionPasswords"/> is enabled <see cref="UniquePassword"/> will generate a random
        /// 100 character password for each compressed file.<br/>
        /// When disabled compressed files will not be password protected.<br/>
        /// </summary>
        /// <value><see langword="false"/></value>
        public bool UniqueCompressionPasswords { get; set; }

        private static Option<bool> NewUniqueCompressionPasswordsOption( Command verbCommand ) {
            Option<bool> pwdCompressOption = new(
                name: "--UniqueCompressionPasswords",
                description: "Enable to password protect files during the compression process prior to " +
                "uploading to all configured cloud providers.",
                getDefaultValue: ( ) => false
            );
            pwdCompressOption.AddAlias( "-ucp" );

            verbCommand.AddOption( pwdCompressOption );

            return pwdCompressOption;
        }

        #endregion UniqueCompressionPasswords


        #region ObfuscateUploadedFileNames

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

        private static Option<bool> NewObfuscateUploadedFileNamesOption( Command verbCommand ) {
            Option<bool> obfuscateOption = new(
                name: "--ObfuscateUploadedFileNames",
                description: "Enable to obfuscate filenames prior to uploading to all configured cloud providers.",
                getDefaultValue: ( ) => false
            );
            obfuscateOption.AddAlias( "-o" );

            verbCommand.AddOption( obfuscateOption );

            return obfuscateOption;
        }

        #endregion ObfuscateUploadedFileNames


        #region EnabledFeatures

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

        private static Option<Cloud_ShareSync_Features> NewEnabledFeaturesOption( Command verbCommand ) {
            Option<Cloud_ShareSync_Features> enabledFeatures = new(
                name: "--EnabledFeatures",
                description: "Specify which features to enable. Available features: " +
                "Backup, Restore, Log4Net, Encryption, Compression, Sqlite, Postgres, BackBlazeB2",
                getDefaultValue: ( ) => (Cloud_ShareSync_Features)242
            );
            enabledFeatures.AddAlias( "-ef" );

            verbCommand.AddOption( enabledFeatures );

            return enabledFeatures;
        }

        #endregion EnabledFeatures


        #region EnabledCloudProviders

        /// <summary>
        /// The list of cloud providers to use in the Cloud-ShareSync process.
        /// </summary>
        /// <value><see cref="CloudProviders.BackBlazeB2"/></value>
        [JsonConverter( typeof( JsonStringEnumConverter ) )]
        public CloudProviders EnabledCloudProviders { get; set; } = CloudProviders.BackBlazeB2;

        private static Option<CloudProviders> NewEnabledCloudProvidersOption( Command verbCommand ) {
            Option<CloudProviders> enabledCloudProviders = new(
                name: "--EnabledCloudProviders",
                description: "Specify which cloud providers to enable. Available cloud providers: BackBlazeB2",
                getDefaultValue: ( ) => CloudProviders.BackBlazeB2
            );
            enabledCloudProviders.AddAlias( "-ecp" );

            verbCommand.AddOption( enabledCloudProviders );

            return enabledCloudProviders;
        }

        #endregion EnabledCloudProviders


        #region VerbHandling

        public static Command NewSyncConfigCommand( Option<FileInfo> configPath ) {
            Command syncConfig = new( "SyncConfig" );
            syncConfig.AddAlias( "syncconfig" );
            syncConfig.AddAlias( "sync" );
            syncConfig.Description = "Edit the Cloud-ShareSync sync config.";

            SetSyncConfigHandler(
                syncConfig,
                NewSyncFolderOption( syncConfig ),
                NewRecurseOption( syncConfig ),
                NewExcludePathsOption( syncConfig ),
                NewWorkingDirectoryOption( syncConfig ),
                NewEncryptBeforeUploadOption( syncConfig ),
                NewCompressBeforeUploadOption( syncConfig ),
                NewUniqueCompressionPasswordsOption( syncConfig ),
                NewObfuscateUploadedFileNamesOption( syncConfig ),
                NewEnabledFeaturesOption( syncConfig ),
                NewEnabledCloudProvidersOption( syncConfig ),
                configPath
            );
            return syncConfig;
        }

        internal static void SetSyncConfigHandler(
            Command syncConfig,
            Option<DirectoryInfo> syncFolder,
            Option<bool> recurse,
            Option<string[]> excludePaths,
            Option<string> workingDirectory,
            Option<bool> encryptBeforeUpload,
            Option<bool> compressBeforeUpload,
            Option<bool> uniqueCompressionPasswords,
            Option<bool> obfuscateUploadedFileNames,
            Option<Cloud_ShareSync_Features> enabledFeatures,
            Option<CloudProviders> enabledCloudProviders,
            Option<FileInfo> configPath
        ) {
            syncConfig.SetHandler( (
                    DirectoryInfo syncFolder,
                    bool recurse,
                    string[] excludePaths,
                    string workingDirectory,
                    bool encryptBeforeUpload,
                    bool compressBeforeUpload,
                    bool uniqueCompressionPasswords,
                    bool obfuscateUploadedFileNames,
                    Cloud_ShareSync_Features enabledFeatures,
                    CloudProviders enabledCloudProviders,
                    FileInfo configPath,
                    InvocationContext ctx,
                    HelpBuilder helpBuilder
                 ) => {

                     if (configPath != null) { ConfigManager.SetAltDefaultConfigPath( configPath.FullName ); }

                     SyncConfig config = new( ) {
                         SyncFolder = syncFolder.FullName,
                         Recurse = recurse,
                         ExcludePaths = excludePaths,
                         WorkingDirectory = workingDirectory,
                         EncryptBeforeUpload = encryptBeforeUpload,
                         CompressBeforeUpload = compressBeforeUpload,
                         UniqueCompressionPasswords = uniqueCompressionPasswords,
                         ObfuscateUploadedFileNames = obfuscateUploadedFileNames,
                         EnabledFeatures = enabledFeatures,
                         EnabledCloudProviders = enabledCloudProviders
                     };

                     new ConfigManager( ).UpdateConfigSection( config );
                 },
                syncFolder,
                recurse,
                excludePaths,
                workingDirectory,
                encryptBeforeUpload,
                compressBeforeUpload,
                uniqueCompressionPasswords,
                obfuscateUploadedFileNames,
                enabledFeatures,
                enabledCloudProviders,
                configPath
            );
        }

        #endregion VerbHandling


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
    }
#nullable enable
}
