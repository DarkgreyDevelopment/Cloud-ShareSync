using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Cloud_ShareSync.Core.BackgroundService.Backup;
using Cloud_ShareSync.Core.BackgroundService.Restore;
using Cloud_ShareSync.Core.Configuration.CommandLine;
using Cloud_ShareSync.Core.Configuration.ManagedActions;

namespace Cloud_ShareSync.GUI.Types {
    internal class CloudShareSyncGUIRootCommand : CloudShareSyncRootCommand {

        public CloudShareSyncGUIRootCommand( ) : base( ) {
            Add( new BackupCommand( ConfigPathOption ) );
            Add( new RestoreCommand( ConfigPathOption ) );
            SetCommandHandler( );
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
                        Program.Start( );
                    }
                },
                ConfigPathOption
            );
        }
    }
}
