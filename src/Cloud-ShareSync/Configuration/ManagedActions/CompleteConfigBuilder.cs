using System.Diagnostics;
using System.Text;
using Cloud_ShareSync.Configuration.CommandLine;
using Cloud_ShareSync.Configuration.Enums;
using Cloud_ShareSync.Configuration.Types;
using Microsoft.Extensions.Configuration;

namespace Cloud_ShareSync.Configuration.ManagedActions {
    public class CompleteConfigBuilder {

        private static readonly ActivitySource s_source = new( "CompleteConfigBuilder" );

        private IConfiguration? _configuration;

        public CompleteConfig BuildCompleteConfig( string configPath, bool provideDefault ) {
            _configuration = GetConfiguration( new( configPath ), provideDefault == false );

            try {
                return BuildConfiguration( );
            } catch {
                if (provideDefault) {
                    return new CompleteConfig( new SyncConfig( ) );
                } else {
                    throw;
                }
            }
        }

        private static IConfiguration GetConfiguration(
            FileInfo appConfig,
            bool requireValidation = true
        ) {
            CompleteConfig? config = null;

            try {
                config = CompleteConfig.FromString( File.ReadAllText( appConfig.FullName ) );
            } catch {
                if (requireValidation) { throw; }
            }

            if (config == null) {
                config = requireValidation == false ?
                    new CompleteConfig( new SyncConfig( ) ) :
                    throw new Exception(
                        $"Could not create CompleteConfig from config file '{appConfig.FullName}'."
                    );
            }

            string jsonString = ValidateAndAssignDefaults( config, requireValidation == false );

            return new ConfigurationBuilder( )
                    .AddEnvironmentVariables( )
                    .AddJsonStream( new MemoryStream( Encoding.ASCII.GetBytes( jsonString ) ) )
                    .Build( );
        }

        public static string ValidateAndAssignDefaults( CompleteConfig config, bool skipValidation ) {
            if (
                skipValidation ||
                config.Sync.SyncFolder == SyncConfig.DefaultSyncFolder
            ) {
                return config.ToString( );
            }

            config.Sync.ValidateSyncConfigDefaults( );
            config.ValidateAndAssignDatabaseDefaults( );
            config.ValidateAndAssignLogDefaults( );
            config.ValidateAndAssignCompressionDefaults( );
            config.ValidateAndAssignBackBlazeDefaults( );
            config.EnsureEncryptionPlatformSupport( );
            return config.ToString( );
        }

        internal IConfigurationSection GetSyncConfigSection( ) => _configuration!.GetRequiredSection( "Sync" );
        internal IConfigurationSection GetB2ConfigSection( ) => _configuration!.GetRequiredSection( "BackBlaze" );
        internal IConfigurationSection GetDatabaseConfigSection( ) => _configuration!.GetRequiredSection( "Database" );
        internal IConfigurationSection GetLog4NetConfigSection( bool required = false ) =>
           required ? _configuration!.GetRequiredSection( "Logging" ) : _configuration!.GetSection( "Logging" );
        internal IConfigurationSection GetCompressionConfigSection( bool required = false ) =>
            required ? _configuration!.GetRequiredSection( "Compression" ) : _configuration!.GetSection( "Compression" );

        private CompleteConfig BuildConfiguration( ) {
            using Activity? activity = s_source.StartActivity( "BuildConfiguration" )?.Start( );

            SyncConfig syncConfig = GetSyncConfigSection( ).Get<SyncConfig>( );

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
                    throw new ArgumentException(
                        "Sync WorkingDirectory path is required when compression or encryption are enabled." );
                } else if (Directory.Exists( sync.WorkingDirectory ) == false) {
                    _ = Directory.CreateDirectory( sync.WorkingDirectory );
                }
            }
        }

        private DatabaseConfig BuildDatabaseConfig( ) => GetDatabaseConfigSection( ).Get<DatabaseConfig>( );

        private Log4NetConfig? BuildLog4NetConfig( SyncConfig sync ) {
            Log4NetConfig? result = null;
            if (sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Log4Net )) {
                result = GetLog4NetConfigSection( true ).Get<Log4NetConfig>( );
            }
            return result;
        }

        private CompressionConfig? BuildCompressionConfig( SyncConfig sync ) {
            CompressionConfig? result = null;
            if (sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.Compression )) {
                result = GetCompressionConfigSection( true ).Get<CompressionConfig>( );
            }
            return result;
        }

        private B2Config? BuildBackBlazeConfig( SyncConfig sync ) {
            B2Config? result = null;
            if (
                sync.EnabledCloudProviders.HasFlag( CloudProviders.BackBlazeB2 ) &&
                sync.EnabledFeatures.HasFlag( Cloud_ShareSync_Features.BackBlazeB2 )
            ) {
                result = GetB2ConfigSection( ).Get<B2Config>( );
            }
            return result;
        }

    }
}
