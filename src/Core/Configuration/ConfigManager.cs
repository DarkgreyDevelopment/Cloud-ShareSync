using System.Diagnostics;
using System.Reflection;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Database;
using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Configuration {

    public class ConfigManager {
        private static readonly ActivitySource s_source = new( "ConfigManager" );
        private static string? s_assemblyPath;

        internal static IConfiguration? Configuration { get; set; }

        public static CompleteConfig GetConfiguration( string[] args ) {
            s_assemblyPath = Directory.GetParent( Assembly.GetExecutingAssembly( ).Location )?.FullName;
            string defaultConfig = Path.Join( s_assemblyPath, "Configuration", "appsettings.json" );
            string? envConfig = Environment.GetEnvironmentVariable( "CLOUDSHARESYNC_CONFIGPATH" );

            string configPath = true switch {
                true when args.Length > 0 && File.Exists( args[0] ) => args[0],
                true when File.Exists( defaultConfig ) => defaultConfig,
                true when envConfig != null && File.Exists( envConfig ) => envConfig,
                _ => throw new InvalidOperationException(
                    "Missing required configuration file. " +
                    "The configuration file path can be specified in one of three ways.\n" +
                    "  1. Pass the path to the configuration file as the first cmdline" +
                    " argument when starting the application.\n" +
                    $"  2. Put the config file in the default config path '{defaultConfig}'.\n" +
                    "  3. Set the 'CLOUDSHARESYNC_CONFIGPATH' environment variable with a valid file path."
                )
            };

            return BuildConfiguration( new( configPath ) );
        }

        public static ILogger ConfigureTelemetryLogger( Log4NetConfig? config, string[] sourceList ) {
            if (config == null) {
                Console.WriteLine(
                    "Log configuration is null. " +
                    "This means that Log4Net was excluded from the Cloud_ShareSync EnabledFeatures. " +
                    "Add Log4Net to the core enabledfeatures to re-enable logging."
                );
            }

            return new TelemetryLogger( sourceList, config );
        }

        public static CloudShareSyncServices ConfigureDatabaseService( DatabaseConfig config, ILogger? log ) {
            using Activity? activity = s_source.StartActivity( "ConfigureDatabaseService" )?.Start( );

            CloudShareSyncServices services = new( config.SqliteDBPath, log );

            SqliteContext sqliteContext = services.Services.GetRequiredService<SqliteContext>( );

            int coreTableCount = (from obj in sqliteContext.CoreData where obj.Id >= 0 select obj).Count( );
            int encryptedCount = (from obj in sqliteContext.EncryptionData where obj.Id >= 0 select obj).Count( );
            int compressdCount = (from obj in sqliteContext.CompressionData where obj.Id >= 0 select obj).Count( );
            int backBlazeCount = (from obj in sqliteContext.BackBlazeB2Data where obj.Id >= 0 select obj).Count( );
            log?.LogInformation( "Database Service Initialized." );
            log?.LogInformation( "Core Table      : {string}", coreTableCount );
            log?.LogInformation( "Encrypted Table : {string}", encryptedCount );
            log?.LogInformation( "Compressed Table: {string}", compressdCount );
            log?.LogInformation( "BackBlaze Table : {string}", backBlazeCount );

            activity?.Stop( );
            return services;
        }

        public static void ValidateConfigSet(
            CompleteConfig config,
            bool restore,
            bool backup,
            ILogger? log

        ) {
            using Activity? activity = s_source.StartActivity( "ValidateConfigSet" )?.Start( );

            if (log != null) { log.LogDebug( "{string}", config.ToString( ) ); }

            if (backup) {
                if (config.Backup == null) {
                    throw new InvalidDataException( "SimpleBackup configuration required." );
                }
            }

            if (restore) {
                if (config.Restore == null) {
                    throw new InvalidDataException( "Restore configuration is required." );
                }
            }

            if (config.Database == null) { throw new InvalidDataException( "Database configuration required." ); }

            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.AwsS3 ) &&
                config.Aws == null
            ) { throw new InvalidDataException( "Aws configuration required." ); }

            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.AzureBlobStorage ) &&
                config.Azure == null
            ) { throw new InvalidDataException( "Azure configuration required." ); }

            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 ) &&
                config.BackBlaze == null
            ) { throw new InvalidDataException( "Backblaze configuration required." ); }

            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.GoogleCloudStorage ) &&
                config.Google == null
            ) { throw new InvalidDataException( "Google configuration required." ); }

            log?.LogInformation( "Configuration Validated." );
            activity?.Stop( );
        }

        public static IConfigurationSection GetSimpleBackup( ) => Configuration.GetRequiredSection( "Backup" );
        public static IConfigurationSection GetRestoreConfig( ) => Configuration.GetRequiredSection( "Restore" );
        public static IConfigurationSection GetBackBlazeB2( ) => Configuration.GetRequiredSection( "BackBlaze" );
        public static IConfigurationSection GetDatabase( ) => Configuration.GetRequiredSection( "Database" );
        public static IConfigurationSection? GetCompression( ) => Configuration?.GetSection( "Compression" );


        #region Private Functions

        private static CompleteConfig BuildConfiguration( FileInfo appConfig ) {
            using Activity? activity = s_source.StartActivity( "BuildConfiguration" );
            activity?.Start( );

            Configuration = new ConfigurationBuilder( )
                                .AddJsonFile( appConfig.FullName )
                                .AddEnvironmentVariables( )
                                .Build( );

            // Get Required Core Settings
            CompleteConfig returnConfig = new(
                Configuration
                .GetRequiredSection( "Core" )
                .Get<CoreConfig>( )
            );

            // If Features Enabled - Get Additional Required Sections
            returnConfig.Backup = BuildBackupConfig( returnConfig.Core );
            returnConfig.Restore = BuildRestoreConfig( returnConfig.Core );
            returnConfig.Database = BuildDatabaseConfig( returnConfig.Core );
            returnConfig.Log4Net = BuildLog4NetConfig( returnConfig.Core );
            returnConfig.Compression = BuildCompressionConfig( returnConfig.Core );
            returnConfig.Aws = BuildAwsConfig( returnConfig.Core );
            returnConfig.Azure = BuildAzConfig( returnConfig.Core );
            returnConfig.BackBlaze = BuildBackBlazeConfig( returnConfig.Core );
            returnConfig.Google = BuildGcpConfig( returnConfig.Core );

            // Validate encryption is supported on this platform if enabled.
            if (
                returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) &&
                Cryptography.FileEncryption.ManagedChaCha20Poly1305.PlatformSupported == false
            ) {
                throw new PlatformNotSupportedException(
                    "This platform does not support ChaCha20Poly1305 cryptography. " +
                    "Remove Encryption from the 'EnabledFeatures' enumeration in the Core config before restarting."
                );
            }

            activity?.Stop( );
            return returnConfig;
        }

        private static BackupConfig? BuildBackupConfig( CoreConfig core ) {
            BackupConfig? result = null;

            if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Backup )) {

                result = GetSimpleBackup( ).Get<BackupConfig>( );

                if (
                    result.EncryptBeforeUpload &&
                    core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) == false
                ) {
                    throw new InvalidOperationException(
                        "Encryption must also be listed as an EnabledFeature in the Core config " +
                        "before setting EncryptBeforeUpload to true."
                    );
                }

                if (
                    result.CompressBeforeUpload &&
                    core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression ) == false
                ) {
                    throw new InvalidOperationException(
                        "Compression must also be listed as an EnabledFeature in the Core config " +
                        "before setting CompressBeforeUpload to true."
                    );
                }

                if (
                    result.CompressBeforeUpload == false &&
                    result.UniqueCompressionPasswords
                ) {
                    // Turn off compression passwords if we're not using compression.
                    result.UniqueCompressionPasswords = false;
                }

                if (Directory.Exists( result.WorkingDirectory ) == false) {
                    Directory.CreateDirectory( result.WorkingDirectory );
                }

                if (Directory.Exists( result.RootFolder ) == false) {
                    throw new DirectoryNotFoundException(
                        "Missing required root folder. " +
                        "Cannot backup files under root folder if it doesn't exist."
                    );
                }
            }

            return result;
        }

        private static RestoreConfig? BuildRestoreConfig( CoreConfig core ) {
            RestoreConfig? result = null;

            if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Restore )) {
                result = GetRestoreConfig( ).Get<RestoreConfig>( );

                if (Directory.Exists( result.WorkingDirectory ) == false) {
                    Directory.CreateDirectory( result.WorkingDirectory );
                }

                if (Directory.Exists( result.RootFolder ) == false) {
                    Directory.CreateDirectory( result.RootFolder );
                }
            }

            return result;
        }

        private static DatabaseConfig BuildDatabaseConfig( CoreConfig core ) {
            // At least one database feature is required!
            if (
                core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false &&
                core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres ) == false
            ) {
                Console.WriteLine( "Adding sqlite to the enabled features list. At least one database feature must be enabled!" );
                core.EnabledFeatures |= Cloud_ShareSync_Features.Sqlite;
            }

            DatabaseConfig? result = null;
            try {
                result = GetDatabase( ).Get<DatabaseConfig>( );
            } catch { }

            // Database configuration cannot be null.
            if (result == null) {
                Console.WriteLine( "Database config is required. Enabling default config." );
                if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false) {
                    Console.WriteLine( "Adding sqlite to the enabled features list." );
                    core.EnabledFeatures |= Cloud_ShareSync_Features.Sqlite;
                }
                result = new DatabaseConfig( ) {
                    UseSqlite = true,
                    SqliteDBPath = "",
                    UsePostgres = false,
                    PostgresConnectionString = ""
                };
            }

            // Sane defaults - at least one db is required!
            if (result.UseSqlite == false && result.UsePostgres == false) {
                Console.WriteLine( "Setting UseSqlite to true. At least one database is required!" );
                if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false) {
                    Console.WriteLine( "Adding sqlite to the enabled features list." );
                    core.EnabledFeatures |= Cloud_ShareSync_Features.Sqlite;
                }
                result.UseSqlite = true;
            }

            if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite )) {
                // Attempt to set sqlite db path is set.
                if (result.UseSqlite && string.IsNullOrWhiteSpace( result.SqliteDBPath )) {
                    result.SqliteDBPath = s_assemblyPath ?? "";
                    Console.WriteLine( $"SqliteDBPath was not set. Set it to '{result.SqliteDBPath}'." );
                }
            }

            if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres )) {
                // Postgres Configuration
                if (result.UsePostgres) {
                    throw new NotImplementedException(
                        "remove Postgres from the core enabled features & set UsePostgres to false."
                    );
                }
            }

            return result;
        }

        private static Log4NetConfig? BuildLog4NetConfig( CoreConfig core ) {
            Log4NetConfig? result = null;
            if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Log4Net )) {
                result = Configuration.GetRequiredSection( "Log4Net" ).Get<Log4NetConfig>( );
            }
            return result;
        }

        private static CompressionConfig? BuildCompressionConfig( CoreConfig core ) {
            CompressionConfig? result = null;

            if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression )) {
                result = Configuration.GetRequiredSection( "Compression" ).Get<CompressionConfig>( );
                if (File.Exists( result.DependencyPath ) == false) {
                    throw new FileNotFoundException(
                        $"Compression is listed as an EnabledFeature but required compression dependency " +
                        $"'{result.DependencyPath}' is missing."
                    );
                }
            }

            return result;
        }

        private static S3Config? BuildAwsConfig( CoreConfig core ) {
            S3Config? result = null;

            if (core.EnabledCloudProviders.HasFlag( CloudProviders.AwsS3 )) {
                throw new NotImplementedException(
                    "AwsS3 CloudProvider Functionality Not Implemented Yet. " +
                    "Remove AwsS3 from the 'EnabledCloudProviders' enumeration in the Core config before restarting."
                );
            }

            return result;
        }

        private static AzConfig? BuildAzConfig( CoreConfig core ) {
            AzConfig? result = null;

            if (core.EnabledCloudProviders.HasFlag( CloudProviders.AzureBlobStorage )) {
                throw new NotImplementedException(
                    "AzureBlobStorage CloudProvider Functionality Not Implemented Yet. " +
                    "Remove AzureBlobStorage from the 'EnabledCloudProviders' enumeration in the Core config before restarting."
                );
            }

            return result;
        }

        private static B2Config? BuildBackBlazeConfig( CoreConfig core ) {
            B2Config? result = null;

            if (
                core.EnabledCloudProviders.HasFlag( CloudProviders.BackBlazeB2 ) &&
                core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 )
            ) {
                result = GetBackBlazeB2( ).Get<B2Config>( );
            }

            return result;
        }

        private static GcsConfig? BuildGcpConfig( CoreConfig core ) {
            GcsConfig? result = null;

            if (core.EnabledCloudProviders.HasFlag( CloudProviders.GoogleCloudStorage )) {
                throw new NotImplementedException(
                    "GoogleCloudStorage CloudProvider Functionality Not Implemented Yet. " +
                    "Remove GoogleCloudStorage from the 'EnabledCloudProviders' enumeration in the Core config before restarting."
                );
            }

            return result;
        }

        #endregion Private Functions

    }
}

