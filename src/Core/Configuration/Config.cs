using System.Diagnostics;
using System.Reflection;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Configuration.Types.Cloud;
using Cloud_ShareSync.Core.Configuration.Types.Features;
using Cloud_ShareSync.Core.Logging.Types;
using Microsoft.Extensions.Configuration;

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

            // Log4Net
            if (returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Log4Net )) {
                returnConfig.Log4Net = Configuration.GetRequiredSection( "Log4Net" ).Get<Log4NetConfig>( );
            }

            // Database
            if (
                returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) ||
                returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres )
            ) {
                returnConfig.Database = Configuration.GetRequiredSection( "Database" ).Get<DatabaseConfig>( );

                // Sqlite
                if (
                    returnConfig.Database.UseSqlite &&
                    string.IsNullOrWhiteSpace( returnConfig.Database.SqliteDBPath )
                ) {
                    returnConfig.Database.SqliteDBPath = s_assemblyPath ?? "";
                }

                // Postgres Configuration
                if (
                        returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres ) ||
                        returnConfig.Database.UsePostgres
                ) {
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

            // Database is required!
            if (
                (
                    returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false &&
                    returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres ) == false
                ) || (
                    returnConfig.Database != null &&
                    returnConfig.Database.UseSqlite == false &&
                    returnConfig.Database.UsePostgres == false
                )
            ) {
                returnConfig.Database = new DatabaseConfig( ) {
                    UseSqlite = true,
                    SqliteDBPath = s_assemblyPath ?? "",
                    UsePostgres = false,
                    PostgresConnectionString = ""
                };
            }

            // Compression
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

            // Encryption
            if (
                returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) &&
                Cryptography.FileEncryption.ManagedChaCha20Poly1305.PlatformSupported == false
            ) {
                throw new PlatformNotSupportedException(
                    "This platform does not support ChaCha20Poly1305 cryptography. " +
                    "Remove Encryption from the 'EnabledFeatures' enumeration in the Core config before restarting."
                );
            }

            // SimpleBackup
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
                    // Don't add unused passwords to the database!
                }
            }

            // RestoreAgent
            if (returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.SimpleRestore )) {
                throw new NotImplementedException( "RestoreAgent Functionality Not Implemented Yet." );
            }


            // Configure Cloud Providers

            // BackBlazeB2
            if (
                returnConfig.Core.EnabledCloudProviders.HasFlag( CloudProviders.BackBlazeB2 ) &&
                returnConfig.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 )
            ) {
                returnConfig.BackBlaze = GetBackBlazeB2( ).Get<B2Config>( );
            }

            // AwsS3
            if (returnConfig.Core.EnabledCloudProviders.HasFlag( CloudProviders.AwsS3 )) {
                throw new NotImplementedException(
                    "AwsS3 CloudProvider Functionality Not Implemented Yet."
                );
            }

            // AzureBlobStorage
            if (returnConfig.Core.EnabledCloudProviders.HasFlag( CloudProviders.AzureBlobStorage )) {
                throw new NotImplementedException(
                    "AzureBlobStorage CloudProvider Functionality Not Implemented Yet."
                );
            }

            // GoogleCloudStorage
            if (returnConfig.Core.EnabledCloudProviders.HasFlag( CloudProviders.GoogleCloudStorage )) {
                throw new NotImplementedException(
                    "GoogleCloudStorage CloudProvider Functionality Not Implemented Yet."
                );
            }

            activity?.Stop( );
            return returnConfig;
        }

        public static IConfigurationSection GetSimpleBackup( ) {
            return Configuration.GetRequiredSection( "SimpleBackup" );
        }

        public static IConfigurationSection GetBackBlazeB2( ) {
            return Configuration.GetRequiredSection( "BackBlaze" );
        }
    }
}

