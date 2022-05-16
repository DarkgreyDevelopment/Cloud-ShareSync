using System.CommandLine;
using Cloud_ShareSync.Core.Configuration.ManagedActions;

namespace Cloud_ShareSync.Core.BackgroundService.Backup {
    public class BackupCommand : Command {
        public BackupCommand( Option<FileInfo> configPath ) : base( name: "Backup" ) {
            AddAlias( "backup" );
            SetBackupCommandHandler( configPath );
        }

        private void SetBackupCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler(
                ( FileInfo path ) => {
                    if (path != null) { ConfigPathHandler.SetAltDefaultConfigPath( path.FullName ); }

                    Process backup = new( );
                    backup.Run( ).GetAwaiter( ).GetResult( );
                },
                configPath
            );
        }
    }
}
