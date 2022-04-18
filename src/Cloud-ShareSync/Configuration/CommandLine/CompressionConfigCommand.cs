using System.CommandLine;
using Cloud_ShareSync.Configuration.ManagedActions;
using Cloud_ShareSync.Configuration.Types;

namespace Cloud_ShareSync.Configuration.CommandLine {
#nullable disable

    public class CompressionConfigCommand : Command {

        public CompressionConfigCommand( Option<FileInfo> configPath ) : base(
            name: "Compression",
            description: "Edit the Cloud-ShareSync compression config."
        ) {
            SetDependencyPathOptionAlias( );
            AddOption( _dependencyPathOption );

            AddAlias( "compression" );
            AddAlias( "7zip" );
            SetCompressionConfigCommandHandler( configPath );
        }

        private readonly Option<string> _dependencyPathOption = new(
                name: "--DependencyPath",
                description: "Specify the path to the 7zip executable."
            ) {
            IsRequired = true
        };

        private void SetDependencyPathOptionAlias( ) { _dependencyPathOption.AddAlias( "-p" ); }

        private void SetCompressionConfigCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler(
                (
                    string dependencyPath,
                    FileInfo configPath
                ) => {
                    if (configPath != null) { ConfigPathHandler.SetAltDefaultConfigPath( configPath.FullName ); }
                    CompressionConfig config = new( ) { DependencyPath = dependencyPath };
                    new ConfigManager( ).UpdateConfigSection( config );
                },
                _dependencyPathOption,
                configPath
            );
        }

    }
#nullable enable
}
