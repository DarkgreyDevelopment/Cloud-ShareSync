using System.CommandLine;

namespace Cloud_ShareSync.Commandline {

    public class Program {

        public static int Main( string[] args ) {
            return new CloudShareSyncCmdRootCommand( ).Invoke( args );
        }
    }
}
