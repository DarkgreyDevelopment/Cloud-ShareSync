using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Cloud_ShareSync.Configuration.ManagedActions;
using Cloud_ShareSync.Configuration.Types;

namespace Cloud_ShareSync.Configuration.CommandLine {
#nullable disable

    public class ConfigureCommand : Command {

        public ConfigureCommand( Option<FileInfo> configPath ) : base( name: "Configure" ) {

            SetCreateConfigOptionAlias( );
            AddOption( _createConfigOption );

            Add( new SyncConfigCommand( configPath ) );
            Add( new DatabaseConfigCommand( configPath ) );
            Add( new CompressionConfigCommand( configPath ) );
            Add( new B2ConfigCommand( configPath ) );
            Add( new Log4NetConfigCommand( configPath ) );
            Add( new TelemetryLogConfigCommand( configPath ) );
            Add( new DefaultLogConfigCommand( configPath ) );
            Add( new ConsoleLogConfigCommand( configPath ) );

            AddAlias( "configure" );
            SetConfigureCommandHandler( configPath );
        }


        private readonly Option<bool> _createConfigOption = new(
                name: "--CreateConfig",
                description: "Use with --ConfigPath to create a new default Cloud-ShareSync configuration file.",
                getDefaultValue: ( ) => false
            ) {
            IsRequired = false
        };

        private void SetCreateConfigOptionAlias( ) {
            _createConfigOption.AddAlias( "--createconfig" );
            _createConfigOption.AddAlias( "-create" );
        }


        private void SetConfigureCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler(
                (
                    FileInfo path,
                    bool create,
                    InvocationContext ctx,
                    HelpBuilder helpBuilder
                ) => {
                    if (path != null) {
                        ConfigPathHandler.SetAltDefaultConfigPath( path.FullName );
                        if (create) {
                            CompleteConfig defaultConfig = new( new( SyncConfig.DefaultSyncFolder ) );
                            Console.WriteLine( $"Writing default Cloud-ShareSync config to '{path.FullName}'." );
                            File.WriteAllText( path.FullName, defaultConfig.ToString( ) );
                        }
                    } else {
                        HelpContext hctx = new( ctx.HelpBuilder, this, Console.Out, null );
                        ctx.HelpBuilder.Write( hctx );
                    }
                },
                configPath,
                _createConfigOption
            );
        }

    }
#nullable enable
}
