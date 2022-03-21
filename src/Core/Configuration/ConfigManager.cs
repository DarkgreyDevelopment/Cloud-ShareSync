using System.Diagnostics;
using System.Text;
using System.Text.Json;
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
        private static readonly string s_assemblyPath = AppContext.BaseDirectory;
        public readonly string ConfigPath;
        public IConfiguration _configuration;

        public ConfigManager( string[] args ) {
            ConfigPath = GetConfigurationPath( args );
            _configuration = GetConfiguration( new( ConfigPath ) );
        }


        #region Public Functions

        public static ILogger CreateTelemetryLogger( Log4NetConfig? config, string[] sourceList ) {
            if (config == null) {
                Console.WriteLine(
                    "Log configuration is null. " +
                    "This means that Log4Net was excluded from the Cloud-ShareSync EnabledFeatures. " +
                    "Add Log4Net to the core enabledfeatures to re-enable logging."
                );
            }

            return new TelemetryLogger( sourceList, config );
        }

        public CompleteConfig BuildConfiguration( ) {
            using Activity? activity = s_source.StartActivity( "BuildConfiguration" )?.Start( );

            // Get Required Core Settings
            CompleteConfig returnConfig = new(
                _configuration
                .GetRequiredSection( "Core" )
                .Get<CoreConfig>( )
            );

            // If Features Enabled - Get Additional Required Sections
            returnConfig.Backup = BuildBackupConfig( returnConfig.Core );
            returnConfig.Restore = BuildRestoreConfig( returnConfig.Core );
            returnConfig.Log4Net = BuildLog4NetConfig( returnConfig.Core );
            returnConfig.Compression = BuildCompressionConfig( returnConfig.Core );
            returnConfig.BackBlaze = BuildBackBlazeConfig( returnConfig.Core );
            returnConfig.Database = BuildDatabaseConfig( );

            activity?.Stop( );
            return returnConfig;
        }

        public static void ValidateConfigSet(
            CompleteConfig config,
            bool restore,
            bool backup,
            ILogger? log
        ) {
            using Activity? activity = s_source.StartActivity( "ValidateConfigSet" )?.Start( );

            if (log != null) {
                log.LogDebug( "{string}", config.ToString( ) );
                SystemMemoryChecker.Inititalize( log );
            }

            if (backup && (config.Backup == null)) {
                throw new InvalidDataException( "SimpleBackup configuration required." );
            }

            if (restore && config.Restore == null) {
                throw new InvalidDataException( "Restore configuration is required." );
            }

            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 ) &&
                config.BackBlaze == null
            ) { throw new InvalidDataException( "Backblaze configuration required." ); }

            log?.LogInformation( "Configuration Validated." );
            activity?.Stop( );
        }

        #endregion Public Functions


        #region Internal Functions

        internal static string GetConfigurationPath( string[] args ) {
            string defaultConfig = Path.Join( s_assemblyPath, "Configuration", "appsettings.json" );
            string? envConfig = Environment.GetEnvironmentVariable( "CLOUDSHARESYNC_CONFIGPATH" );

            return true switch {
                true when args.Length > 0 && File.Exists( args[0] ) => args[0],
                true when File.Exists( defaultConfig ) => defaultConfig,
                true when envConfig != null && File.Exists( envConfig ) => envConfig,
                _ => throw new ApplicationException(
                    "Missing required configuration file. " +
                    "The configuration file path can be specified in one of three ways.\n" +
                    "  1. Pass the path to the configuration file as the first cmdline" +
                    " argument when starting the application.\n" +
                    $"  2. Put the config file in the default config path '{defaultConfig}'.\n" +
                    "  3. Set the 'CLOUDSHARESYNC_CONFIGPATH' environment variable with a valid file path."
                )
            };
        }

        internal static CloudShareSyncServices ConfigureDatabaseService( DatabaseConfig config, ILogger? log ) {
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

        internal IConfigurationSection GetSimpleBackup( ) => _configuration.GetRequiredSection( "Backup" );
        internal IConfigurationSection GetRestoreConfig( ) => _configuration.GetRequiredSection( "Restore" );
        internal IConfigurationSection GetBackBlazeB2( ) => _configuration.GetRequiredSection( "BackBlaze" );
        internal IConfigurationSection GetDatabase( ) => _configuration.GetRequiredSection( "Database" );
        internal IConfigurationSection GetCompression( bool required = false ) =>
            required ? _configuration.GetRequiredSection( "Compression" ) : _configuration.GetSection( "Compression" );

        #endregion Internal Functions


        #region Private Functions

        private static IConfiguration GetConfiguration( FileInfo appConfig ) {
            CompleteConfig? config = JsonSerializer
                                        .Deserialize<CompleteConfig>(
                                            File.ReadAllText( appConfig.FullName ),
                                            new JsonSerializerOptions( ) {
                                                ReadCommentHandling = JsonCommentHandling.Skip
                                            }
                                        );
            string jsonString = ValidateAndAssignDefaults( config, appConfig.FullName );

            return new ConfigurationBuilder( )
                                .AddEnvironmentVariables( )
                                .AddJsonStream( new MemoryStream( Encoding.ASCII.GetBytes( jsonString ) ) )
                                .Build( );
        }

        private static string ValidateAndAssignDefaults( CompleteConfig? config, string configPath ) {
            string errTxt = $"\nUpdate '{configPath}' to change the applications settings.";

            if (config == null) { config = new CompleteConfig( new CoreConfig( ) ); }

            if (config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Log4Net )) {
                if (config.Log4Net == null) { config.Log4Net = new( ); }

                if (
                    string.IsNullOrWhiteSpace( config.Log4Net.ConfigurationFile ) == false
                ) {
                    if (File.Exists( config.Log4Net.ConfigurationFile ) == false) {
                        throw new FileNotFoundException(
                            $"Cannot find Log4Net ConfigurationFile '{config.Log4Net.ConfigurationFile}'." +
                            errTxt
                        );
                    } else {
                        if (config.Log4Net.EnableTelemetryLog) {
                            if (config.Log4Net.DefaultLogConfiguration == null) {
                                Console.WriteLine(
                                    "DefaultLogConfiguration was unset and EnableDefaultLog is true. " +
                                    "Adding default values." +
                                    errTxt
                                );
                                config.Log4Net.DefaultLogConfiguration = new( );
                            }
                            if (Directory.Exists( config.Log4Net.DefaultLogConfiguration.LogDirectory ) == false) {
                                _ = Directory.CreateDirectory( config.Log4Net.DefaultLogConfiguration.LogDirectory );
                            }
                        }

                        if (config.Log4Net.EnableTelemetryLog) {
                            if (config.Log4Net.TelemetryLogConfiguration == null) {
                                Console.WriteLine(
                                    "TelemetryLogConfiguration was unset and EnableTelemetryLog is true. " +
                                    "Adding default values." +
                                    errTxt
                                );
                                config.Log4Net.TelemetryLogConfiguration = new( );
                            }
                            if (Directory.Exists( config.Log4Net.TelemetryLogConfiguration.LogDirectory ) == false) {
                                _ = Directory.CreateDirectory( config.Log4Net.TelemetryLogConfiguration.LogDirectory );
                            }
                        }
                    }
                }

            }

            // DATABASE
            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false &&
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres ) == false
            ) {
                Console.WriteLine(
                    "At least one database feature must be enabled! Adding sqlite to the enabled features list." +
                    errTxt
                );
                config.Core.EnabledFeatures |= Cloud_ShareSync_Features.Sqlite;
            }
            if (config.Database == null) {
                Console.WriteLine(
                    "DatabaseConfig is empty. Setting default values." +
                    errTxt
                );
                config.Database = new( );
                config.Core.EnabledFeatures |= Cloud_ShareSync_Features.Sqlite;
            } else {
                // Sane defaults - at least one db is required!
                if (config.Database.UseSqlite == false && config.Database.UsePostgres == false) {
                    Console.WriteLine(
                        "At least one database is required! Setting UseSqlite to true." +
                        errTxt
                    );
                    config.Database.UseSqlite = true;
                }

                if (
                    config.Database.UseSqlite == true &&
                    config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false
                ) {
                    Console.WriteLine(
                        "Adding sqlite to the enabled features list." +
                        errTxt
                    );
                    config.Core.EnabledFeatures |= Cloud_ShareSync_Features.Sqlite;
                }

                if (
                    config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) &&
                    config.Database.UseSqlite &&
                    string.IsNullOrWhiteSpace( config.Database.SqliteDBPath )
                ) {
                    // Attempt to set sqlite db path is set.
                    config.Database.SqliteDBPath = s_assemblyPath;
                    Console.WriteLine(
                        $"SqliteDBPath was not set. Setting it to '{config.Database.SqliteDBPath}'." +
                        errTxt
                    );
                }

                if (
                    config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres ) &&
                    config.Database.UsePostgres
                ) {
                    // Postgres Configuration
                    throw new NotImplementedException(
                        "Remove Postgres from the core enabled features & set UsePostgres to false." +
                        errTxt
                    );
                }
            }

            // BACKUP
            if (config.Backup != null) {
                if (
                    config.Backup.CompressBeforeUpload == false &&
                    config.Backup.UniqueCompressionPasswords
                ) {
                    Console.WriteLine(
                        "Disabling UniqueCompressionPasswords because CompressBeforeUpload is false." +
                        errTxt
                    );
                    // Turn off compression passwords if we're not using compression.
                    config.Backup.UniqueCompressionPasswords = false;
                }

                if (
                    config.Backup.EncryptBeforeUpload &&
                    config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) == false
                ) {
                    throw new ApplicationException(
                        "Encryption must also be listed as an EnabledFeature in the Core config " +
                        "before setting EncryptBeforeUpload to true." +
                        errTxt
                    );
                }

                if (
                    config.Backup.CompressBeforeUpload &&
                    config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression ) == false
                ) {
                    throw new ApplicationException(
                        "Compression must also be listed as an EnabledFeature in the Core config " +
                        "before setting CompressBeforeUpload to true." +
                        errTxt
                    );
                }

                if (Directory.Exists( config.Backup.RootFolder ) == false) {
                    throw new DirectoryNotFoundException(
                        "Missing required root folder. " +
                        "Cannot backup files under root folder if it doesn't exist." +
                        errTxt
                    );
                }
            }

            // ENCRYPTION
            if (
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) &&
                Cryptography.FileEncryption.ManagedChaCha20Poly1305.PlatformSupported == false
            ) {
                throw new PlatformNotSupportedException(
                    "This platform does not support ChaCha20Poly1305 cryptography. " +
                    "Remove Encryption from the 'EnabledFeatures' enumeration in the Core config before restarting." +
                    errTxt
                );
            }

            // COMPRESSION
            if (config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression )) {
                if (File.Exists( config.Compression?.DependencyPath ) != true) {
                    throw new FileNotFoundException(
                        $"Compression is listed as an EnabledFeature but required compression dependency " +
                        $"'{config.Compression?.DependencyPath}' is missing." +
                        errTxt
                    );
                }
            }

            ValidateConfigSet( config,
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Restore ),
                config.Core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Backup ),
                null
            );

            return config.ToString( );
        }

        private BackupConfig? BuildBackupConfig( CoreConfig core ) {
            BackupConfig? result = null;

            if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Backup )) {

                result = GetSimpleBackup( ).Get<BackupConfig>( );

                if (Directory.Exists( result.WorkingDirectory ) == false) {
                    _ = Directory.CreateDirectory( result.WorkingDirectory );
                }
            }

            return result;
        }

        private RestoreConfig? BuildRestoreConfig( CoreConfig core ) {
            RestoreConfig? result = null;

            if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Restore )) {
                result = GetRestoreConfig( ).Get<RestoreConfig>( );

                if (Directory.Exists( result.WorkingDirectory ) == false) {
                    _ = Directory.CreateDirectory( result.WorkingDirectory );
                }

                if (Directory.Exists( result.RootFolder ) == false) {
                    _ = Directory.CreateDirectory( result.RootFolder );
                }
            }

            return result;
        }

        private DatabaseConfig BuildDatabaseConfig( ) {
            DatabaseConfig result = GetDatabase( ).Get<DatabaseConfig>( );
            return result;
        }

        private Log4NetConfig? BuildLog4NetConfig( CoreConfig core ) {
            Log4NetConfig? result = null;
            if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Log4Net )) {
                result = _configuration.GetRequiredSection( "Log4Net" ).Get<Log4NetConfig>( );
            }
            return result;
        }

        private CompressionConfig? BuildCompressionConfig( CoreConfig core ) {
            CompressionConfig? result = null;

            if (core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression )) {
                result = GetCompression( true ).Get<CompressionConfig>( );
            }

            return result;
        }

        private B2Config? BuildBackBlazeConfig( CoreConfig core ) {
            B2Config? result = null;

            if (
                core.EnabledCloudProviders.HasFlag( CloudProviders.BackBlazeB2 ) &&
                core.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 )
            ) {
                result = GetBackBlazeB2( ).Get<B2Config>( );
            }

            return result;
        }

        #endregion Private Functions

    }
}

