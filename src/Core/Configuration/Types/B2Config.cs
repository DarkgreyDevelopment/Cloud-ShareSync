using System.CommandLine;
using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Interfaces;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// <para>
    /// Required configuration values to connect to a BackBlazeB2 bucket.
    /// </para>
    /// Reference: <see href="https://help.backblaze.com/hc/en-us/articles/360052129034-Creating-and-Managing-Application-Keys">BackBlaze - Creating and Managing Application Keys</see>
    /// </summary>
    public class B2Config : ICloudProviderConfig {

        #region ApplicationKeyId

        /// <summary>
        /// The "keyID" associated with the BackBlaze B2 <see cref="ApplicationKey"/>.
        /// </summary>
        public string ApplicationKeyId { get; set; }

        private static Option<string> NewApplicationKeyIdOption( Command verbCommand ) {
            Option<string> applicationKeyIdOption = new(
                name: "--ApplicationKeyId",
                description: "Specify the backblaze ApplicationKeyId."
            );
            applicationKeyIdOption.AddAlias( "-id" );
            applicationKeyIdOption.IsRequired = true;

            verbCommand.AddOption( applicationKeyIdOption );

            return applicationKeyIdOption;
        }

        #endregion ApplicationKeyId


        #region ApplicationKey

        /// <summary>
        /// The value for the BackBlaze B2 api key.
        /// </summary>
        public string ApplicationKey { get; set; }

        private static Option<string> NewApplicationKeyOption( Command verbCommand ) {
            Option<string> applicationKeyOption = new(
                name: "--ApplicationKey",
                description: "Specify the backblaze ApplicationKey."
            );
            applicationKeyOption.AddAlias( "-ak" );
            applicationKeyOption.IsRequired = true;

            verbCommand.AddOption( applicationKeyOption );

            return applicationKeyOption;
        }

        #endregion ApplicationKey


        #region BucketName

        /// <summary>
        /// The name of the BackBlaze B2 storage "bucket".
        /// </summary>
        public string BucketName { get; set; }

        private static Option<string> NewBucketNameOption( Command verbCommand ) {
            Option<string> bucketNameOption = new(
                name: "--BucketName",
                description: "Specify the backblaze BucketName."
            );
            bucketNameOption.AddAlias( "-bn" );
            bucketNameOption.IsRequired = true;

            verbCommand.AddOption( bucketNameOption );

            return bucketNameOption;
        }

        #endregion BucketName


        #region BucketId

        /// <summary>
        /// The id of the BackBlaze B2 storage "bucket".
        /// </summary>
        public string BucketId { get; set; }

        private static Option<string> NewBucketIdOption( Command verbCommand ) {
            Option<string> bucketIdOption = new(
                name: "--BucketId",
                description: "Specify the backblaze BucketId."
            );
            bucketIdOption.AddAlias( "-bid" );
            bucketIdOption.IsRequired = true;

            verbCommand.AddOption( bucketIdOption );

            return bucketIdOption;
        }

        #endregion BucketId


        #region MaxConsecutiveErrors

        /// <summary>
        /// The number of consecutive errors to receive before aborting an upload/download.
        /// </summary>
        /// <value>10</value>
        public int MaxConsecutiveErrors { get; set; } = 10;

        private static Option<int> NewMaxConsecutiveErrorsOption( Command verbCommand ) {
            Option<int> maxConsecutiveErrorsOption = new(
                name: "--MaxConsecutiveErrors",
                description: "Specify the number of consecutive errors to receive before aborting an upload/download.",
                getDefaultValue: ( ) => 10
            );
            maxConsecutiveErrorsOption.AddAlias( "-e" );
            maxConsecutiveErrorsOption.IsRequired = false;

            verbCommand.AddOption( maxConsecutiveErrorsOption );

            return maxConsecutiveErrorsOption;
        }

        #endregion MaxConsecutiveErrors


        #region ProcessThreads

        /// <summary>
        /// The number of concurrent connections to open for large file uploads or downloads.
        /// </summary>
        /// <value>50</value>
        public int ProcessThreads { get; set; } = 50;

        private static Option<int> NewProcessThreadsOption( Command verbCommand ) {
            Option<int> processThreads = new(
                name: "--ProcessThreads",
                description: "Specify the number of concurrent connections to open for large file uploads or downloads.",
                getDefaultValue: ( ) => 50
            );
            processThreads.AddAlias( "-t" );
            processThreads.IsRequired = false;

            verbCommand.AddOption( processThreads );

            return processThreads;
        }

        #endregion ProcessThreads


        #region VerbHandling

        public static Command NewB2ConfigCommand( Option<FileInfo> configPath ) {
            Command backBlazeB2Config = new( "BackBlaze" );
            backBlazeB2Config.AddAlias( "backblaze" );
            backBlazeB2Config.AddAlias( "b2" );
            backBlazeB2Config.Description =
                "Configure the required configuration values to connect to a BackBlazeB2 bucket.";

            SetCompressionConfigHandler(
                backBlazeB2Config,
                NewApplicationKeyIdOption( backBlazeB2Config ),
                NewApplicationKeyOption( backBlazeB2Config ),
                NewBucketNameOption( backBlazeB2Config ),
                NewBucketIdOption( backBlazeB2Config ),
                NewMaxConsecutiveErrorsOption( backBlazeB2Config ),
                NewProcessThreadsOption( backBlazeB2Config ),
                configPath
            );
            return backBlazeB2Config;
        }

        internal static void SetCompressionConfigHandler(
            Command backBlazeB2Config,
            Option<string> applicationKeyId,
            Option<string> applicationKey,
            Option<string> bucketName,
            Option<string> bucketId,
            Option<int> maxConsecutiveErrors,
            Option<int> processThreads,
            Option<FileInfo> configPath
        ) {
            backBlazeB2Config.SetHandler( (
                     string applicationKeyId,
                     string applicationKey,
                     string bucketName,
                     string bucketId,
                     int maxConsecutiveErrors,
                     int processThreads,
                     FileInfo configPath
                 ) => {
                     if (configPath != null) { ConfigManager.SetAltDefaultConfigPath( configPath.FullName ); }

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
                applicationKeyId,
                applicationKey,
                bucketName,
                bucketId,
                maxConsecutiveErrors,
                processThreads,
                configPath
            );
        }

        #endregion VerbHandling


        /// <summary>
        /// Public Parameterless Constructor - Requires manual assignment of all non-default values.<br/>
        /// Used in the IConfiguration import process.
        /// </summary>
        public B2Config( ) { }

        /// <summary>
        /// Returns the <see cref="B2Config"/> as a json string.
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
