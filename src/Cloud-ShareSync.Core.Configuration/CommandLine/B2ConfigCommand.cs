using System.CommandLine;
using Cloud_ShareSync.Core.Configuration.ManagedActions;
using Cloud_ShareSync.Core.Configuration.Types;

namespace Cloud_ShareSync.Core.Configuration.CommandLine {
#nullable disable

    public class B2ConfigCommand : Command {

        public B2ConfigCommand( Option<FileInfo> configPath ) : base(
            name: "BackBlaze",
            description: "Configure the required configuration values to connect to a BackBlazeB2 bucket."
        ) {

            SetApplicationKeyIdOptionAlias( );
            AddOption( _applicationKeyIdOption );

            SetApplicationKeyOptionAlias( );
            AddOption( _applicationKeyOption );

            SetBucketNameOptionAlias( );
            AddOption( _bucketNameOption );

            SetBucketIdOptionAlias( );
            AddOption( _bucketIdOption );

            SetMaxConsecutiveErrorsOptionAlias( );
            AddOption( _maxConsecutiveErrorsOption );

            SetProcessThreadsOptionAlias( );
            AddOption( _processThreadsOption );

            AddAlias( "backblaze" );
            AddAlias( "b2" );

            SetB2ConfigCommandHandler( configPath );
        }

        private readonly Option<string> _applicationKeyIdOption = new(
                name: "--ApplicationKeyId",
                description: "Specify the backblaze ApplicationKeyId."
            ) {
            IsRequired = true
        };

        private void SetApplicationKeyIdOptionAlias( ) { _applicationKeyIdOption.AddAlias( "-id" ); }


        private readonly Option<string> _applicationKeyOption = new(
                name: "--ApplicationKey",
                description: "Specify the backblaze ApplicationKey."
            ) {
            IsRequired = true
        };

        private void SetApplicationKeyOptionAlias( ) { _applicationKeyOption.AddAlias( "-ak" ); }


        private readonly Option<string> _bucketNameOption = new(
                name: "--BucketName",
                description: "Specify the backblaze BucketName."
            ) {
            IsRequired = true
        };

        private void SetBucketNameOptionAlias( ) { _bucketNameOption.AddAlias( "-bn" ); }


        private readonly Option<string> _bucketIdOption = new(
                name: "--BucketId",
                description: "Specify the backblaze BucketId."
            ) {
            IsRequired = true
        };

        private void SetBucketIdOptionAlias( ) { _bucketIdOption.AddAlias( "-bid" ); }


        private readonly Option<int> _maxConsecutiveErrorsOption = new(
                name: "--MaxConsecutiveErrors",
                description: "Specify the number of consecutive errors to receive before aborting an upload/download.",
                getDefaultValue: ( ) => 10
            ) {
            IsRequired = false
        };

        private void SetMaxConsecutiveErrorsOptionAlias( ) { _maxConsecutiveErrorsOption.AddAlias( "-e" ); }


        private readonly Option<int> _processThreadsOption = new(
                name: "--ProcessThreads",
                description: "Specify the number of concurrent connections to open for large file uploads or downloads.",
                getDefaultValue: ( ) => 50
            ) {
            IsRequired = false
        };

        private void SetProcessThreadsOptionAlias( ) { _processThreadsOption.AddAlias( "-t" ); }

        private void SetB2ConfigCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler(
                (
                    string applicationKeyId,
                    string applicationKey,
                    string bucketName,
                    string bucketId,
                    int maxConsecutiveErrors,
                    int processThreads,
                    FileInfo configPath
                ) => {
                    if (configPath != null) { ConfigPathHandler.SetAltDefaultConfigPath( configPath.FullName ); }

                    B2Config config = new( ) {
                        ApplicationKeyId = applicationKeyId,
                        ApplicationKey = applicationKey,
                        BucketName = bucketName,
                        BucketId = bucketId,
                        MaxConsecutiveErrors = maxConsecutiveErrors,
                        ProcessThreads = processThreads
                    };
                    new ConfigManager( ).UpdateConfigSection( config );
                },
                _applicationKeyIdOption,
                _applicationKeyOption,
                _bucketNameOption,
                _bucketIdOption,
                _maxConsecutiveErrorsOption,
                _processThreadsOption,
                configPath
            );
        }

    }
#nullable enable
}
