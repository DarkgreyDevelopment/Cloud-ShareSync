using System.CommandLine;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.ManagedActions;
using Cloud_ShareSync.Core.Configuration.Types;

namespace Cloud_ShareSync.Core.Configuration.CommandLine {
#nullable disable

    public class SyncConfigCommand : Command {

        public SyncConfigCommand( Option<FileInfo> configPath ) : base(
            name: "SyncConfig",
            description: "Edit the Cloud-ShareSync sync config."
        ) {
            AddAliases( );
            AddOptions( );
            SetSyncConfigCommandHandler( configPath );
        }

        #region Ctor Configuration Methods

        private void AddAliases( ) {
            AddOptionsAliases( );
            AddAlias( "syncconfig" );
            AddAlias( "sync" );
        }

        private void AddOptionsAliases( ) {
            SetSyncFolderOptionAlias( );
            SetRecurseOptionAlias( );
            SetExcludePathsOptionAlias( );
            SetWorkingDirectoryOptionAlias( );
            SetEncryptBeforeUploadOptionAlias( );
            SetCompressBeforeUploadOptionAlias( );
            SetUniqueCompressionPasswordsOptionAlias( );
            SetObfuscateUploadedFileNamesOptionAlias( );
            SetEnabledFeaturesOptionAlias( );
            SetEnabledCloudProvidersOptionAlias( );
        }

        private void AddOptions( ) {
            AddOption( _syncFolderOption );
            AddOption( _recurseOption );
            AddOption( _excludePathsOption );
            AddOption( _workingDirectoryOption );
            AddOption( _encryptBeforeUploadOption );
            AddOption( _compressBeforeUploadOption );
            AddOption( _uniqueCompressionPasswordsOption );
            AddOption( _obfuscateUploadedFileNamesOption );
            AddOption( _enabledFeaturesOption );
            AddOption( _enabledCloudProvidersOption );
        }

        #endregion Ctor Configuration Methods

        #region Options

        private readonly Option<DirectoryInfo> _syncFolderOption = new(
                name: "--SyncFolder",
                description: "Specify the path to the root directory to sync with configured cloud providers."
            ) {
            IsRequired = false
        };

        private void SetSyncFolderOptionAlias( ) { _syncFolderOption.AddAlias( "-sf" ); }


        private readonly Option<bool> _recurseOption = new(
                name: "--Recurse",
                description: "Enable to sync files in directories below the SyncFolder.",
                getDefaultValue: ( ) => true
            ) {
            IsRequired = false
        };

        private void SetRecurseOptionAlias( ) { _recurseOption.AddAlias( "-r" ); }


        private readonly Option<string[]> _excludePathsOption = new(
                name: "--ExcludePaths",
                description: "Specify paths to exclude from the sync process.",
                getDefaultValue: ( ) => Array.Empty<string>( )
            ) {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true
        };

        private void SetExcludePathsOptionAlias( ) { _excludePathsOption.AddAlias( "-ep" ); }


        private readonly Option<string> _workingDirectoryOption = new(
                name: "--WorkingDirectory",
                description: "Specify the path to a directory to use as a working directory " +
                "when compressing or encrypting files, as well as for file decompression and decryption.",
                getDefaultValue: ( ) => string.Empty
            ) {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true
        };

        private void SetWorkingDirectoryOptionAlias( ) { _workingDirectoryOption.AddAlias( "-w" ); }


        private readonly Option<bool> _compressBeforeUploadOption = new(
                name: "--CompressBeforeUpload",
                description: "Enable to compress files prior to uploading to all configured cloud providers.",
                getDefaultValue: ( ) => false
            ) {
            IsRequired = false
        };

        private void SetCompressBeforeUploadOptionAlias( ) { _compressBeforeUploadOption.AddAlias( "-c" ); }


        private readonly Option<bool> _uniqueCompressionPasswordsOption = new(
                name: "--UniqueCompressionPasswords",
                description: "Enable to password protect files during the compression process prior to " +
                "uploading to all configured cloud providers.",
                getDefaultValue: ( ) => false
            ) {
            IsRequired = false
        };

        private void SetUniqueCompressionPasswordsOptionAlias( ) { _uniqueCompressionPasswordsOption.AddAlias( "-ucp" ); }


        private readonly Option<bool> _encryptBeforeUploadOption = new(
                name: "--EncryptBeforeUpload",
                description: "Enable to encrypt files prior to uploading to all configured cloud providers.",
                getDefaultValue: ( ) => false
            ) {
            IsRequired = false
        };

        private void SetEncryptBeforeUploadOptionAlias( ) { _encryptBeforeUploadOption.AddAlias( "-eu" ); }


        private readonly Option<bool> _obfuscateUploadedFileNamesOption = new(
                name: "--ObfuscateUploadedFileNames",
                description: "Enable to obfuscate filenames prior to uploading to all configured cloud providers.",
                getDefaultValue: ( ) => false
            ) {
            IsRequired = false
        };

        private void SetObfuscateUploadedFileNamesOptionAlias( ) { _obfuscateUploadedFileNamesOption.AddAlias( "-o" ); }


        private readonly Option<Cloud_ShareSync_Features> _enabledFeaturesOption = new(
                name: "--EnabledFeatures",
                description: "Specify which features to enable. Available features: " +
                "Backup, Restore, Log4Net, Encryption, Compression, Sqlite, Postgres, BackBlazeB2",
                getDefaultValue: ( ) => (Cloud_ShareSync_Features)242
            ) {
            IsRequired = false
        };

        private void SetEnabledFeaturesOptionAlias( ) { _enabledFeaturesOption.AddAlias( "-ef" ); }


        private readonly Option<CloudProviders> _enabledCloudProvidersOption = new(
                name: "--EnabledCloudProviders",
                description: "Specify which cloud providers to enable. Available cloud providers: BackBlazeB2",
                getDefaultValue: ( ) => CloudProviders.BackBlazeB2
            ) {
            IsRequired = false
        };

        private void SetEnabledCloudProvidersOptionAlias( ) { _enabledCloudProvidersOption.AddAlias( "-ecp" ); }


        #endregion Options

        private void SetSyncConfigCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler( (
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
                    FileInfo configPath
                 ) => {

                     if (configPath != null) { ConfigPathHandler.SetAltDefaultConfigPath( configPath.FullName ); }

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
                _syncFolderOption,
                _recurseOption,
                _excludePathsOption,
                _workingDirectoryOption,
                _encryptBeforeUploadOption,
                _compressBeforeUploadOption,
                _uniqueCompressionPasswordsOption,
                _obfuscateUploadedFileNamesOption,
                _enabledFeaturesOption,
                _enabledCloudProvidersOption,
                configPath
            );
        }

    }
#nullable enable
}
