using Cloud_ShareSync.Core.BackgroundService.Backup;
using Cloud_ShareSync.Core.BackgroundService.Restore;
using Cloud_ShareSync.Core.Configuration.CommandLine;

namespace Cloud_ShareSync.Commandline {
    internal class CloudShareSyncCmdRootCommand : CloudShareSyncRootCommand {

        public CloudShareSyncCmdRootCommand( ) : base( ) {
            Add( new BackupCommand( ConfigPathOption ) );
            Add( new RestoreCommand( ConfigPathOption ) );
        }
    }
}
