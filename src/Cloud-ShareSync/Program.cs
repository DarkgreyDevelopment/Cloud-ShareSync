using System.CommandLine;
using Cloud_ShareSync.Configuration.CommandLine;

namespace Cloud_ShareSync {

    public class Program {

        public static int Main( string[] args ) {
            return new CloudShareSyncRootCommand( ).Invoke( args );
        }
    }
}
