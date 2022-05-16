using System.CommandLine;
using Cloud_ShareSync.Core.Configuration.ManagedActions;

namespace Cloud_ShareSync.Core.BackgroundService.Restore {
    public class RestoreCommand : Command {
        public RestoreCommand( Option<FileInfo> configPath ) : base( name: "Restore" ) {
            AddAlias( "restore" );
            SetRestoreCommandHandler( configPath );
        }

        private void SetRestoreCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler(
                ( FileInfo path ) => {
                    if (path != null) { ConfigPathHandler.SetAltDefaultConfigPath( path.FullName ); }

                    Process restore = new( );
                    restore.Run( ).GetAwaiter( ).GetResult( );
                },
                configPath
            );
        }
    }
}
