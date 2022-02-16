using System.Diagnostics;
using System.Reflection;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Database.Sqlite;
using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Configuration {

    public class Config {
        private static readonly ActivitySource s_source = new( "Config" );
        private static string? s_assemblyPath;

        public static IConfiguration? Configuration { get; set; }

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

            // If Features Enabled - Get Additional Required Sections.

            #region Log4Net

            if (returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Log4Net )) {
                returnConfig.Log4Net = Configuration.GetRequiredSection( "Log4Net" ).Get<Log4NetConfig>( );
            }

            #endregion Log4Net


            #region Database Config

            // At least one database feature is required!
            if (
                returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false &&
                returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres ) == false
            ) {
                Console.WriteLine( "Adding sqlite to the enabled features list. At least one database feature must be enabled!" );
                returnConfig.Core.EnabledFeatures = returnConfig.Core.EnabledFeatures | Cloud_ShareSync_Features.Sqlite;
            }

            returnConfig.Database = GetDatabase( ).Get<DatabaseConfig>( ); ;

            // Sane defaults - at least one db is required!
            if (
                returnConfig.Database != null &&
                returnConfig.Database.UseSqlite == false &&
                returnConfig.Database.UsePostgres == false
            ) {
                Console.WriteLine( "Setting UseSqlite to true. At least one database is required!" );
                if (returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false) {
                    Console.WriteLine( "Adding sqlite to the enabled features list." );
                    returnConfig.Core.EnabledFeatures = returnConfig.Core.EnabledFeatures | Cloud_ShareSync_Features.Sqlite;
                }
                returnConfig.Database.UseSqlite = true;
            }

            // Database configuration cannot be null.
            if (returnConfig.Database == null) {
                Console.WriteLine( "Database config is required. Enabling default config." );
                if (returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false) {
                    Console.WriteLine( "Adding sqlite to the enabled features list." );
                    returnConfig.Core.EnabledFeatures = returnConfig.Core.EnabledFeatures | Cloud_ShareSync_Features.Sqlite;
                }
                returnConfig.Database = new DatabaseConfig( ) {
                    UseSqlite = true,
                    SqliteDBPath = "",
                    UsePostgres = false,
                    PostgresConnectionString = ""
                };
            }

            if (returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite )) {
                // Attempt to set sqlite db path is set.
                if (returnConfig.Database.UseSqlite && string.IsNullOrWhiteSpace( returnConfig.Database.SqliteDBPath )) {
                    returnConfig.Database.SqliteDBPath = s_assemblyPath ?? "";
                    Console.WriteLine( $"SqliteDBPath was not set. Set it to '{returnConfig.Database.SqliteDBPath}'." );
                }
            }

            if (returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres )) {
                // Postgres Configuration
                if (returnConfig.Database.UsePostgres) {
                    throw new NotImplementedException(
                        "remove Postgres from the core enabled features & set UsePostgres to false."
                    );

                    //if (string.IsNullOrWhiteSpace( returnConfig.Database.PostgresConnectionString )) {
                    //    throw new InvalidDataException(
                    //        "PostgresConnectionString is required to use postgres. " +
                    //        "Either add a valid connection string or" +
                    //        "remove Postgres from the core enabled features & " +
                    //        "set UsePostgres to false."
                    //    );
                    //}
                }
            }

            #endregion Database Config


            #region Compression

            if (returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression )) {

                returnConfig.Compression = Configuration.GetRequiredSection( "Compression" ).Get<CompressionConfig>( );
                if (File.Exists( returnConfig.Compression.DependencyPath ) == false) {
                    throw new FileNotFoundException(
                        $"Compression is listed as an EnabledFeature but required compression dependency " +
                        $"'{returnConfig.Compression.DependencyPath}' is missing."
                    );
                }

                if (Directory.Exists( returnConfig.Compression.InterimZipPath ) == false) {
                    Directory.CreateDirectory( returnConfig.Compression.InterimZipPath );
                }
            }

            #endregion Compression


            #region Encryption

            if (
                returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) &&
                Cryptography.FileEncryption.ManagedChaCha20Poly1305.PlatformSupported == false
            ) {
                throw new PlatformNotSupportedException(
                    "This platform does not support ChaCha20Poly1305 cryptography. " +
                    "Remove Encryption from the 'EnabledFeatures' enumeration in the Core config before restarting."
                );
            }

            #endregion Encryption


            #region SimpleBackup

            if (returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.SimpleBackup )) {

                returnConfig.SimpleBackup = GetSimpleBackup( ).Get<BackupConfig>( );

                if (
                    returnConfig.SimpleBackup.EncryptBeforeUpload &&
                    returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) == false
                ) {
                    throw new InvalidOperationException(
                        "Encryption must also be listed as an EnabledFeature in the Core config " +
                        "before setting EncryptBeforeUpload to true."
                    );
                }

                if (
                    returnConfig.SimpleBackup.CompressBeforeUpload &&
                    returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression ) == false
                ) {
                    throw new InvalidOperationException(
                        "Compression must also be listed as an EnabledFeature in the Core config " +
                        "before setting CompressBeforeUpload to true."
                    );
                }

                if (
                    returnConfig.SimpleBackup.CompressBeforeUpload == false &&
                    returnConfig.SimpleBackup.UniqueCompressionPasswords
                ) {
                    returnConfig.SimpleBackup.UniqueCompressionPasswords = false;
                }
            }

            #endregion SimpleBackup


            #region SimpleRestore

            if (returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.SimpleRestore )) {
                throw new NotImplementedException( "RestoreAgent Functionality Not Implemented Yet." );
            }

            #endregion SimpleRestore


            #region Cloud Providers

            #region BackBlazeB2

            if (
                returnConfig.Core.EnabledCloudProviders.HasFlag( CloudProviders.BackBlazeB2 ) &&
                returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 )
            ) {
                returnConfig.BackBlaze = GetBackBlazeB2( ).Get<B2Config>( );
            }

            #endregion BackBlazeB2

            #region AwsS3

            if (returnConfig.Core.EnabledCloudProviders.HasFlag( CloudProviders.AwsS3 )) {
                throw new NotImplementedException(
                    "AwsS3 CloudProvider Functionality Not Implemented Yet."
                );
            }

            #endregion AwsS3

            #region AzureBlobStorage

            if (returnConfig.Core.EnabledCloudProviders.HasFlag( CloudProviders.AzureBlobStorage )) {
                throw new NotImplementedException(
                    "AzureBlobStorage CloudProvider Functionality Not Implemented Yet."
                );
            }

            #endregion AzureBlobStorage

            #region GoogleCloudStorage

            if (returnConfig.Core.EnabledCloudProviders.HasFlag( CloudProviders.GoogleCloudStorage )) {
                throw new NotImplementedException(
                    "GoogleCloudStorage CloudProvider Functionality Not Implemented Yet."
                );
            }

            #endregion GoogleCloudStorage

            #endregion Cloud Providers

            activity?.Stop( );
            return returnConfig;
        }

        public static IConfigurationSection GetSimpleBackup( ) => Configuration.GetRequiredSection( "SimpleBackup" );
        public static IConfigurationSection GetBackBlazeB2( ) => Configuration.GetRequiredSection( "BackBlaze" );
        public static IConfigurationSection GetDatabase( ) => Configuration.GetRequiredSection( "Database" );
        public static IConfigurationSection? GetCompression( ) => Configuration?.GetSection( "Compression" );

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
            ILogger? log,
            bool restore,
            bool backup,
            bool logConfig = true
        ) {
            using Activity? activity = s_source.StartActivity( "ValidateConfigSet" )?.Start( );

            if (logConfig) { log?.LogInformation( "{string}", config.ToString( ) ); }

            // Configure SystemMemoryChecker
            SystemMemoryChecker.Inititalize( log );
            SystemMemoryChecker.Update( );

            if (backup) {
                if (config.SimpleBackup == null) {
                    throw new InvalidDataException( "SimpleBackup configuration required." );
                }

                if (Directory.Exists( config.SimpleBackup.WorkingDirectory )) {
                    log?.LogInformation( "Working Directory Exists" );
                } else {
                    throw new DirectoryNotFoundException(
                        $"Working directory '{config.SimpleBackup.WorkingDirectory}' doesn't exist." );
                }
            }

            if (restore) {
                if (config.SimpleRestore == null) {
                    throw new InvalidDataException( "Restore configuration is required." );
                }

                if (Directory.Exists( config.SimpleRestore.WorkingDirectory )) {
                    log?.LogInformation( "Working Directory Exists" );
                } else {
                    throw new DirectoryNotFoundException(
                        $"Working directory '{config.SimpleRestore.WorkingDirectory}' doesn't exist." );
                }

                if (Directory.Exists( config.SimpleRestore.RootFolder )) {
                    log?.LogInformation( "Root Folder Exists" );
                } else {
                    throw new DirectoryNotFoundException(
                        $"Root directory '{config.SimpleRestore.RootFolder}' doesn't exist." );
                }
            }

            if (config.Database == null) { throw new InvalidDataException( "Database configuration required." ); }

            /*
            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.AwsS3 ) &&
                config.Aws == null
            ) { throw new InvalidDataException( "Aws configuration required." ); }
            */

            /*
            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.AzureBlobStorage ) &&
                config.Azure == null
            ) { throw new InvalidDataException( "Azure configuration required." ); }
            */

            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 ) &&
                config.BackBlaze == null
            ) { throw new InvalidDataException( "Backblaze configuration required." ); }

            /*
            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.GoogleCloudStorage ) &&
                config.Google == null
            ) { throw new InvalidDataException( "Google configuration required." ); }
            */

            log?.LogInformation( "Configuration Validated." );
            activity?.Stop( );
        }
    }
}

