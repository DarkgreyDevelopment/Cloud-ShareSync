using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.Interfaces;
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
        private static readonly string s_defaultConfig = Path.Join( s_assemblyPath, "appsettings.json" );
        private static readonly string s_altConfigInfo = Path.Join( s_assemblyPath, ".configpath" );
        public string _configPath;
        public IConfiguration _configuration;

        internal readonly CompleteConfig Config;

        public ConfigManager( ) {
            _configPath = GetConfigurationPath( );
            _configuration = GetConfiguration( new( _configPath ) );
            Config = BuildConfiguration( );
        }

        #region UpdateConfigSection

        internal void UpdateConfigSection( ICloudShareSyncConfig configSection ) {

            UpdateB2ConfigSection( configSection );
            UpdateCompressionConfigSection( configSection );
            UpdateConsoleLogConfigSection( configSection );
            UpdateDatabaseConfigSection( configSection );
            UpdateDefaultLogConfigSection( configSection );
            UpdateLog4NetConfigSection( configSection );
            UpdateSyncConfigSection( configSection );
            UpdateTelemetryLogConfigSection( configSection );

            WriteUpdatedConfig( );
        }

        internal void UpdateB2ConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as B2Config) != null) {
                Config.BackBlaze = (B2Config)configSection;
            }
        }

        internal void UpdateCompressionConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as CompressionConfig) != null) {
                Config.Compression = (CompressionConfig)configSection;
            }
        }

        internal void UpdateConsoleLogConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as ConsoleLogConfig) != null) {
                if (Config.Logging == null) {
                    Config.Logging = new Log4NetConfig( false ) { EnableConsoleLog = true };
                }
                Config.Logging.ConsoleConfiguration = (ConsoleLogConfig)configSection;
            }
        }

        internal void UpdateDatabaseConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as DatabaseConfig) != null) {
                Config.Database = (DatabaseConfig)configSection;
            }
        }

        internal void UpdateDefaultLogConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as DefaultLogConfig) != null) {
                if (Config.Logging == null) {
                    Config.Logging = new Log4NetConfig( false ) { EnableDefaultLog = true };
                }
                Config.Logging.DefaultLogConfiguration = (DefaultLogConfig)configSection;
            }
        }

        internal void UpdateLog4NetConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as Log4NetConfig) != null) {
                Log4NetConfig section = (Log4NetConfig)configSection;
                if (Config.Logging == null) {
                    Config.Logging = section;
                } else {
                    Config.Logging.ConfigurationFile = section.ConfigurationFile;
                    Config.Logging.EnableDefaultLog = section.EnableDefaultLog;
                    Config.Logging.EnableTelemetryLog = section.EnableTelemetryLog;
                    Config.Logging.EnableConsoleLog = section.EnableConsoleLog;
                }
            }
        }

        internal void UpdateSyncConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as SyncConfig) != null) {
                Config.Sync = (SyncConfig)configSection;
            }
        }

        internal void UpdateTelemetryLogConfigSection( ICloudShareSyncConfig configSection ) {
            if ((configSection as TelemetryLogConfig) != null) {
                if (Config.Logging == null) {
                    Config.Logging = new Log4NetConfig( false ) { EnableTelemetryLog = true };
                }
                Config.Logging.TelemetryLogConfiguration = (TelemetryLogConfig)configSection;
            }
        }

        internal void WriteUpdatedConfig( ) {
            Console.WriteLine( $"Writing Cloud-ShareSync config to '{_configPath}'." );
            File.WriteAllText( _configPath, Config.ToString( ) );
        }

        #endregion UpdateConfigSection


        #region Initialize ConfigManager

        internal static string GetConfigurationPath( ) {
            string? envConfig = Environment.GetEnvironmentVariable( "CLOUDSHARESYNC_CONFIGPATH" );
            string? altConfigPath = GetAltDefaultConfigPath( );

            return true switch {
                true when altConfigPath != null && File.Exists( altConfigPath ) => altConfigPath,
                true when File.Exists( s_defaultConfig ) => s_defaultConfig,
                true when envConfig != null && File.Exists( envConfig ) => envConfig,
                _ => throw new ApplicationException(
                    "Missing required configuration file. " +
                    "The configuration file path can be specified in one of three ways.\n" +
                    "  1. Pass the path to the configuration file via the --ConfigPath cmdline " +
                    "option. Using the --ConfigPath option will set a new default config location.\n" +
                    "     When using --ConfigPath ensure the path specified exists.\n" +
                    $"  2. Put the config file in the default config path '{s_defaultConfig}'. " +
                    "  3. Set the 'CLOUDSHARESYNC_CONFIGPATH' environment variable with a valid file path.\n" +
                    "Use the Configure commandline option to customize the config. " +
                    "See 'Cloud-ShareSync Configure -h' for more information." +
                    (altConfigPath != null ? $"\nSpecified ConfigPath '{altConfigPath}' does not exist." : "")
                )
            };
        }

        #region Alternate Default Config

        internal static string? GetAltDefaultConfigPath( ) {
            if (File.Exists( s_altConfigInfo )) {
                string path = ReadAltDefaultConfigInfo( File.ReadAllText( s_altConfigInfo ) );
                if (File.Exists( path ) == false) {
                    Console.WriteLine(
                        $"Missing Alternate Default Config Path: {path}\n" +
                        $"The file path specified in '{s_altConfigInfo}' does not exist. " +
                        "This may lead to errors unless an alternate is specified via --ConfigPath."
                    );
                }
                return path;
            } else {
                return null;
            }
        }

        internal static string ReadAltDefaultConfigInfo( string base64EncodedData ) =>
            Encoding.UTF8.GetString( Convert.FromBase64String( base64EncodedData ) );

        internal static void SetAltDefaultConfigPath( string path ) {
            Console.WriteLine( $"Setting default config path to '{path}'." );
            string base64path = Convert.ToBase64String( Encoding.UTF8.GetBytes( path ) );
            File.WriteAllText( s_altConfigInfo, base64path );
        }

        #endregion Alternate Default Config

        internal static IConfiguration GetConfiguration( FileInfo appConfig ) {
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

        #region Validate and Assign Defaults

        private static string ValidateAndAssignDefaults( CompleteConfig? config, string configPath ) {
            string errTxt = $"\nUpdate '{configPath}' to change the applications settings.";
            if (config == null) { config = new CompleteConfig( new SyncConfig( ) ); }
            if (config.Sync.SyncFolder == SyncConfig.DefaultSyncFolder) { return config.ToString( ); }
            config = ValidateAndAssignLogDefaults( config, errTxt );
            config = ValidateAndAssignDatabaseDefaults( config, errTxt );
            config = ValidateAndAssignSyncDefaults( config, errTxt );
            config = ValidateAndAssignEncryptionDefaults( config, errTxt );
            config = ValidateAndAssignCompressionDefaults( config, errTxt );
            config = ValidateAndAssignBackBlazeDefaults( config, errTxt );
            return config.ToString( );
        }

        private static CompleteConfig ValidateAndAssignLogDefaults( CompleteConfig config, string errTxt ) {
            if (config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Log4Net )) {
                if (config.Logging == null) { config.Logging = new( ); }
                if (
                    string.IsNullOrWhiteSpace( config.Logging.ConfigurationFile ) == false &&
                    File.Exists( config.Logging.ConfigurationFile ) == false
                ) {
                    throw new FileNotFoundException(
                        $"Cannot find Log4Net ConfigurationFile '{config.Logging.ConfigurationFile}'." +
                        errTxt
                    );
                } else {
                    if (config.Logging.EnableTelemetryLog) {
                        if (config.Logging.DefaultLogConfiguration == null) {
                            Console.WriteLine(
                                "DefaultLogConfiguration was unset and EnableDefaultLog is true. " +
                                "Adding default values." +
                                errTxt
                            );
                            config.Logging.DefaultLogConfiguration = new( );
                        }
                        if (Directory.Exists( config.Logging.DefaultLogConfiguration.LogDirectory ) == false) {
                            _ = Directory.CreateDirectory( config.Logging.DefaultLogConfiguration.LogDirectory );
                        }
                    }

                    if (config.Logging.EnableTelemetryLog) {
                        if (config.Logging.TelemetryLogConfiguration == null) {
                            Console.WriteLine(
                                "TelemetryLogConfiguration was unset and EnableTelemetryLog is true. " +
                                "Adding default values." +
                                errTxt
                            );
                            config.Logging.TelemetryLogConfiguration = new( );
                        }
                        if (Directory.Exists( config.Logging.TelemetryLogConfiguration.LogDirectory ) == false) {
                            _ = Directory.CreateDirectory( config.Logging.TelemetryLogConfiguration.LogDirectory );
                        }
                    }
                }
            }
            return config;
        }

        private static CompleteConfig ValidateAndAssignDatabaseDefaults( CompleteConfig config, string errTxt ) {
            if (
                config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false &&
                config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres ) == false
            ) {
                Console.WriteLine(
                    "At least one database feature must be enabled! Adding sqlite to the enabled features list." +
                    errTxt
                );
                config.Sync.EnabledFeatures |= Cloud_ShareSync_Features.Sqlite;
            }
            if (config.Database == null) {
                Console.WriteLine(
                    "DatabaseConfig is empty. Setting default values." +
                    errTxt
                );
                config.Database = new( );
                config.Sync.EnabledFeatures |= Cloud_ShareSync_Features.Sqlite;
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
                    config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) == false
                ) {
                    Console.WriteLine(
                        "Adding sqlite to the enabled features list." +
                        errTxt
                    );
                    config.Sync.EnabledFeatures |= Cloud_ShareSync_Features.Sqlite;
                }

                if (
                    config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Sqlite ) &&
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
                    config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Postgres ) &&
                    config.Database.UsePostgres
                ) {
                    // Postgres Configuration
                    throw new NotImplementedException(
                        "Remove Postgres from the sync enabled features & set UsePostgres to false." +
                        errTxt
                    );
                }
            }

            return config;
        }

        private static CompleteConfig ValidateAndAssignSyncDefaults( CompleteConfig config, string errTxt ) {

            if (config.Sync.CompressBeforeUpload == false && config.Sync.UniqueCompressionPasswords) {
                Console.WriteLine( "Disabling UniqueCompressionPasswords because CompressBeforeUpload is false." + errTxt );
                // Turn off compression passwords if we're not using compression.
                config.Sync.UniqueCompressionPasswords = false;
            }

            if (
                config.Sync.EncryptBeforeUpload &&
                config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) == false
            ) {
                throw new ApplicationException(
                    "Encryption must also be listed as an EnabledFeature in the Sync config " +
                    "before setting EncryptBeforeUpload to true." +
                    errTxt
                );
            }

            if (
                config.Sync.CompressBeforeUpload &&
                config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression ) == false
            ) {
                throw new ApplicationException(
                    "Compression must also be listed as an EnabledFeature in the Sync config " +
                    "before setting CompressBeforeUpload to true." +
                    errTxt
                );
            }

            if (Directory.Exists( config.Sync.SyncFolder ) == false) {
                throw new DirectoryNotFoundException(
                    "Missing required SyncFolder. " +
                    $"Cannot sync files under '{config.Sync.SyncFolder}' if the folder doesn't exist." +
                    errTxt
                );
            }

            return config;
        }

        private static CompleteConfig ValidateAndAssignEncryptionDefaults( CompleteConfig config, string errTxt ) =>
            config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption ) &&
            Cryptography.FileEncryption.ManagedChaCha20Poly1305.PlatformSupported == false ?
                throw new PlatformNotSupportedException(
                    "This platform does not support ChaCha20Poly1305 cryptography. " +
                    "Remove Encryption from the 'EnabledFeatures' enumeration in the Sync config before restarting." +
                    errTxt ) :
                config;

        private static CompleteConfig ValidateAndAssignCompressionDefaults( CompleteConfig config, string errTxt ) =>
            config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression ) &&
            File.Exists( config.Compression?.DependencyPath ) != true ?
                throw new FileNotFoundException(
                    $"Compression is listed as an EnabledFeature but required compression dependency " +
                    $"'{config.Compression?.DependencyPath}' is missing." + errTxt ) :
                config;

        private static CompleteConfig ValidateAndAssignBackBlazeDefaults( CompleteConfig config, string errTxt ) =>
            config.Sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 ) && config.BackBlaze == null ?
                throw new InvalidDataException(
                    "Missing required BackBlaze configuration section. " +
                    "Either add the required BackBlaze configuration or remove BackBlazeB2 from the 'EnabledFeatures' " +
                    "enumeration in the Sync config before restarting." + errTxt ) :
                config;

        #endregion Validate and Assign Defaults

        #endregion Initialize ConfigManager


        #region CreateTelemetryLogger

        public static ILogger CreateTelemetryLogger( Log4NetConfig? config, string[] sourceList ) {
            if (config == null) {
                Console.WriteLine(
                    "Log configuration is null. " +
                    "This means that Log4Net was excluded from the Cloud-ShareSync EnabledFeatures. " +
                    "Add Log4Net to the sync enabledfeatures to re-enable logging."
                );
            }

            return new TelemetryLogger( sourceList, config );
        }

        #endregion CreateTelemetryLogger


        #region ConfigureDatabaseService

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

        #endregion ConfigureDatabaseService


        #region Get Configuration Section

        internal IConfigurationSection GetSyncConfig( ) => _configuration.GetRequiredSection( "Sync" );
        internal IConfigurationSection GetBackBlazeB2( ) => _configuration.GetRequiredSection( "BackBlaze" );
        internal IConfigurationSection GetDatabase( ) => _configuration.GetRequiredSection( "Database" );
        internal IConfigurationSection GetLogging( ) => _configuration.GetRequiredSection( "Logging" );
        internal IConfigurationSection GetCompression( bool required = false ) =>
            required ? _configuration.GetRequiredSection( "Compression" ) : _configuration.GetSection( "Compression" );

        #endregion Get Configuration Section


        #region Build Configuration

        public CompleteConfig BuildConfiguration( ) {
            using Activity? activity = s_source.StartActivity( "BuildConfiguration" )?.Start( );

            SyncConfig syncConfig = GetSyncConfig( ).Get<SyncConfig>( );

            // Get Required Sync Settings
            CompleteConfig returnConfig = new( syncConfig );

            ConfigureWorkingDirectory( syncConfig );

            // If Features Enabled - Get Additional Required Sections
            returnConfig.Logging = BuildLog4NetConfig( syncConfig );
            returnConfig.Compression = BuildCompressionConfig( syncConfig );
            returnConfig.BackBlaze = BuildBackBlazeConfig( syncConfig );
            returnConfig.Database = BuildDatabaseConfig( );

            activity?.Stop( );
            return returnConfig;
        }

        private static void ConfigureWorkingDirectory( SyncConfig sync ) {
            if (
                sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression ) ||
                sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Encryption )
            ) {
                if (string.IsNullOrWhiteSpace( sync.WorkingDirectory )) {
                    throw new ArgumentException( "Sync WorkingDirectory is required when compression or encryption are enabled." );
                } else if (Directory.Exists( sync.WorkingDirectory ) == false) {
                    _ = Directory.CreateDirectory( sync.WorkingDirectory );
                }
            }
        }

        private DatabaseConfig BuildDatabaseConfig( ) => GetDatabase( ).Get<DatabaseConfig>( );

        private Log4NetConfig? BuildLog4NetConfig( SyncConfig sync ) {
            Log4NetConfig? result = null;
            if (sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Log4Net )) {
                result = GetLogging( ).Get<Log4NetConfig>( );
            }
            return result;
        }

        private CompressionConfig? BuildCompressionConfig( SyncConfig sync ) {
            CompressionConfig? result = null;
            if (sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression )) {
                result = GetCompression( true ).Get<CompressionConfig>( );
            }
            return result;
        }

        private B2Config? BuildBackBlazeConfig( SyncConfig sync ) {
            B2Config? result = null;
            if (
                sync.EnabledCloudProviders.HasFlag( CloudProviders.BackBlazeB2 ) &&
                sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 )
            ) {
                result = GetBackBlazeB2( ).Get<B2Config>( );
            }
            return result;
        }

        #endregion Build Configuration

    }
}

