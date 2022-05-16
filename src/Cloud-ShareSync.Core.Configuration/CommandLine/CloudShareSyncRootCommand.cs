using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Cloud_ShareSync.Core.Configuration.ManagedActions;

namespace Cloud_ShareSync.Core.Configuration.CommandLine {
    public class CloudShareSyncRootCommand : RootCommand {

        public CloudShareSyncRootCommand( ) {
            Description = "Cloud-ShareSync";
            SetConfigPathOptionAlias( );
            AddGlobalOption( ConfigPathOption );
            SetCommandHandler( );

            Add( new ConfigureCommand( ConfigPathOption ) );
        }

        public readonly Option<FileInfo> ConfigPathOption = new(
            name: "--ConfigPath",
            description: "The path to the applications appsettings.json file."
        ) {
            IsRequired = false
        };

        private void SetConfigPathOptionAlias( ) {
            ConfigPathOption.AddAlias( "--configpath" );
            ConfigPathOption.AddAlias( "-path" );
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
                    }
                },
                ConfigPathOption
            );
        }
    }
}
