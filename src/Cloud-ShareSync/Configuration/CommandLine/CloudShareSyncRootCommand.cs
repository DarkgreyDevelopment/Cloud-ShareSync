using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Cloud_ShareSync.Configuration.ManagedActions;

namespace Cloud_ShareSync.Configuration.CommandLine {
    internal class CloudShareSyncRootCommand : RootCommand {

        public CloudShareSyncRootCommand( ) {
            Description = "Cloud-ShareSync";
            SetConfigPathOptionAlias( );
            AddGlobalOption( _configPathOption );
            SetCommandHandler( );

            Add( new ConfigureCommand( _configPathOption ) );
            //AddBackupCommand( rootCommand, option );
            //AddRestoreCommand( rootCommand, option );
        }

        private readonly Option<FileInfo> _configPathOption = new(
            name: "--ConfigPath",
            description: "The path to the applications appsettings.json file."
        ) {
            IsRequired = false
        };

        private void SetConfigPathOptionAlias( ) {
            _configPathOption.AddAlias( "--configpath" );
            _configPathOption.AddAlias( "-path" );
        }

        private void SetCommandHandler( ) {
            this.SetHandler(
                (
                    FileInfo path,
                    InvocationContext ctx,
                    HelpBuilder helpBuilder
                ) => {
                    if (path != null) {
                        ConfigPathHandler.SetAltDefaultConfigPath( path.FullName );
                    } else {
                        HelpContext hctx = new( ctx.HelpBuilder, this, Console.Out, null );
                        ctx.HelpBuilder.Write( hctx );
                        GUI.Program.Main( Array.Empty<string>( ) );
                    }
                },
                _configPathOption
            );
        }
    }
}
