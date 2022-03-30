using System.CommandLine;
using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Interfaces;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// Configuration settings to use when compression has been enabled.
    /// </summary>
    public class CompressionConfig : ICloudShareSyncConfig {

        #region DependencyPath

        /// <summary>
        /// Specify the path to the 7zip executable.
        /// </summary>
        public string DependencyPath { get; set; }

        private static Option<string> NewDependencyPathOption( Command verbCommand ) {
            Option<string> dependencyPathOption = new(
                name: "--DependencyPath",
                description: "Specify the path to the 7zip executable."
            );
            dependencyPathOption.AddAlias( "-p" );
            dependencyPathOption.IsRequired = true;

            verbCommand.AddOption( dependencyPathOption );

            return dependencyPathOption;
        }

        #endregion DependencyPath


        #region VerbHandling

        public static Command NewCompressionConfigCommand( Option<FileInfo> configPath ) {
            Command compressionConfig = new( "Compression" );
            compressionConfig.AddAlias( "compression" );
            compressionConfig.AddAlias( "7zip" );
            compressionConfig.Description = "Edit the Cloud-ShareSync compression config.";

            SetCompressionConfigHandler(
                compressionConfig,
                NewDependencyPathOption( compressionConfig ),
                configPath
            );
            return compressionConfig;
        }

        internal static void SetCompressionConfigHandler(
            Command databaseConfig,
            Option<string> dependencyPath,
            Option<FileInfo> configPath
        ) {
            databaseConfig.SetHandler( (
                    string dependencyPath,
                    FileInfo configPath
                ) => {
                    if (configPath != null) { ConfigManager.SetAltDefaultConfigPath( configPath.FullName ); }
                    CompressionConfig config = new( ) { DependencyPath = dependencyPath };
                    new ConfigManager( ).UpdateConfigSection( config );
                },
                dependencyPath,
                configPath
            );
        }

        #endregion VerbHandling


        /// <summary>
        /// Returns the <see cref="CompressionConfig"/> as a json string.
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
