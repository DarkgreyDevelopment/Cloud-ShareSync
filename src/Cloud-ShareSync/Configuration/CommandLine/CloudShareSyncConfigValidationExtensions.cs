using Cloud_ShareSync.Configuration.Enums;
using Cloud_ShareSync.Configuration.Types;

namespace Cloud_ShareSync.Configuration.CommandLine {
    internal static class CloudShareSyncConfigValidationExtensions {

        #region SyncConfig Extensions

        /// <summary>
        /// Extension method to set defaults or throw an error if required conditions have not been met.
        /// </summary>
        /// <param name="config"></param>
        internal static void ValidateSyncConfigDefaults( this SyncConfig config ) {
            ValidateUniqueCompressionPasswordsConfig( config );
            ValidateEncryptBeforeUploadConfig( config );
            ValidateCompressBeforeUploadConfig( config );
            EnsureSyncFolderExists( config );
        }

        private static void ValidateUniqueCompressionPasswordsConfig( SyncConfig config ) {
            if (config.CompressBeforeUpload == false && config.UniqueCompressionPasswords) {
                Console.WriteLine(
                    "Disabling UniqueCompressionPasswords because CompressBeforeUpload is false."
                );
                // Turn off compression passwords if we're not using compression.
                config.UniqueCompressionPasswords = false;
            }
        }

        private static void ValidateEncryptBeforeUploadConfig( SyncConfig config ) {
            if (
                config.EncryptBeforeUpload &&
                config.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) == false
            ) {
                throw new ApplicationException(
                    "Encryption must also be listed as an EnabledFeature in the Sync config " +
                    "before setting EncryptBeforeUpload to true."
                );
            }
        }

        private static void ValidateCompressBeforeUploadConfig( SyncConfig config ) {
            if (
                config.CompressBeforeUpload &&
                config.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression ) == false
            ) {
                throw new ApplicationException(
                    "Compression must also be listed as an EnabledFeature in the Sync config " +
                    "before setting CompressBeforeUpload to true."
                );
            }
        }

        private static void EnsureSyncFolderExists( SyncConfig config ) {
            if (Directory.Exists( config.SyncFolder ) == false) {
                throw new DirectoryNotFoundException(
                    "Missing required SyncFolder. " +
                    $"Cannot sync files under '{config.SyncFolder}' if the folder doesn't exist."
                );
            }
        }

        #endregion SyncConfig Extensions


        #region DatabaseConfig Extensions

        internal static void ValidateAndAssignDatabaseDefaults( this CompleteConfig config ) {
            EnsureDatabaseFeaturesEnabled( config.Sync );
            EnsureDatabaseIsUsed( config.Database );
            EnsureSqliteFeatureEnabled( config.Sync, config.Database );
            EnsureSqliteDBPathIsSet( config.Sync, config.Database );

            if (
                config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres ) &&
                config.Database.UsePostgres
            ) {
                // Postgres Configuration
                throw new NotImplementedException(
                    "Remove Postgres from the sync enabled features & set UsePostgres to false."
                );
            }
        }

        private static void EnsureDatabaseFeaturesEnabled( SyncConfig config ) {
            if (
                config.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false &&
                config.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres ) == false
            ) {
                Console.WriteLine(
                    "At least one database feature must be enabled! Adding sqlite to the enabled features list."
                );
                config.EnabledFeatures |= Cloud_ShareSync_Features.Sqlite;
            }
        }

        private static void EnsureDatabaseIsUsed( DatabaseConfig config ) {
            // Sane defaults - at least one db is required!
            if (config.UseSqlite == false && config.UsePostgres == false) {
                Console.WriteLine(
                    "At least one database is required! Setting UseSqlite to true."
                );
                config.UseSqlite = true;
            }
        }

        private static void EnsureSqliteFeatureEnabled(
            SyncConfig syncConfig,
            DatabaseConfig databaseconfig
        ) {
            if (
                databaseconfig.UseSqlite == true &&
                syncConfig.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false
            ) {
                throw new Exception(
                    "UseSqlite is set to true but Sqlite is not in the the enabled features list. " +
                    "Add Sqlite to the enabled features in the sync settings before restarting."
                );
            }
        }

        private static void EnsureSqliteDBPathIsSet(
            SyncConfig syncConfig,
            DatabaseConfig databaseconfig
        ) {
            if (
                syncConfig.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) &&
                databaseconfig.UseSqlite &&
                string.IsNullOrWhiteSpace( databaseconfig.SqliteDBPath )
            ) {
                throw new Exception(
                    $"SqliteDBPath was not set. Ensure the database path is set to a secure location on the filesystem."
                );
            }
        }

        #endregion DatabaseConfig Extensions


        #region Log4NetConfig Extensions

        internal static void ValidateAndAssignLogDefaults( this CompleteConfig config ) {
            if (config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Log4Net )) {
                Log4NetConfig log4netConfig = EnsureLogConfigExists( config );
                if (
                    string.IsNullOrWhiteSpace( log4netConfig.ConfigurationFile ) == false &&
                    File.Exists( log4netConfig.ConfigurationFile ) == false
                ) {
                    throw new FileNotFoundException(
                        $"Cannot find Log4Net ConfigurationFile '{log4netConfig.ConfigurationFile}'."
                    );
                } else {
                    ValidateDefaultLogSettings( log4netConfig );
                    ValidateTelemetryLogSettings( log4netConfig );
                }
                config.Logging = log4netConfig;
            }
        }

        private static Log4NetConfig EnsureLogConfigExists( CompleteConfig config ) {
            if (config.Logging == null) {
                Console.WriteLine(
                    "Log4Net is in the enabled features list but the Log config was unset. " +
                    "Enabling default Logging configuration section."
                );
                return new( );
            }

            return config.Logging;
        }

        private static void ValidateDefaultLogSettings( Log4NetConfig log4netConfig ) {
            if (log4netConfig.EnableDefaultLog) {
                if (log4netConfig.DefaultLogConfiguration == null) {
                    Console.WriteLine(
                        "DefaultLogConfiguration was unset and EnableDefaultLog is true. " +
                        "Adding default values."
                    );
                    log4netConfig.DefaultLogConfiguration = new( );
                }
                if (Directory.Exists( log4netConfig.DefaultLogConfiguration.LogDirectory ) == false) {
                    _ = Directory.CreateDirectory( log4netConfig.DefaultLogConfiguration.LogDirectory );
                }
            }
        }

        private static void ValidateTelemetryLogSettings( Log4NetConfig log4netConfig ) {
            if (log4netConfig.EnableTelemetryLog) {
                if (log4netConfig.TelemetryLogConfiguration == null) {
                    Console.WriteLine(
                        "TelemetryLogConfiguration was unset and EnableTelemetryLog is true. " +
                        "Adding default values."
                    );
                    log4netConfig.TelemetryLogConfiguration = new( );
                }
                if (Directory.Exists( log4netConfig.TelemetryLogConfiguration.LogDirectory ) == false) {
                    _ = Directory.CreateDirectory( log4netConfig.TelemetryLogConfiguration.LogDirectory );
                }
            }
        }

        #endregion Log4NetConfig Extensions


        #region CompressionConfig Extensions

        internal static void ValidateAndAssignCompressionDefaults( this CompleteConfig config ) {
            if (
                config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression ) &&
                File.Exists( config.Compression?.DependencyPath ) != true
            ) {
                throw new FileNotFoundException(
                    $"Compression is listed as an EnabledFeature but required compression dependency " +
                    $"'{config.Compression?.DependencyPath}' is missing."
                );
            }
        }

        #endregion CompressionConfig Extensions


        #region B2Config Extensions

        internal static void ValidateAndAssignBackBlazeDefaults( this CompleteConfig config ) {
            if (
                config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 ) &&
                config.BackBlaze == null
            ) {
                throw new InvalidDataException(
                    "Missing required BackBlaze configuration section. " +
                    "Either add the required BackBlaze configuration or remove BackBlazeB2 from the 'EnabledFeatures' " +
                    "enumeration in the Sync config before restarting."
                );
            }
        }

        #endregion B2Config Extensions


        #region Encryption Config Setting Extensions

        internal static void EnsureEncryptionPlatformSupport( this CompleteConfig config ) {
            if (
                config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) &&
                Core.Cryptography.FileEncryption.ManagedChaCha20Poly1305.PlatformSupported == false
            ) {
                throw new PlatformNotSupportedException(
                    "This platform does not support ChaCha20Poly1305 cryptography. " +
                    "Remove Encryption from the 'EnabledFeatures' enumeration in the Sync config before restarting."
                );
            }
        }

        #endregion Encryption Config Setting Extensions

    }
}
