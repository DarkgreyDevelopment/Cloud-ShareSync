using System.CommandLine;
using Cloud_ShareSync.Core.Configuration;

namespace Cloud_ShareSync {

    public class Program {

        public static int Main( string[] args ) {
            RootCommand rootCommand = new( );
            CommandlineConfigurator.ConfigureCommandlineOptions( rootCommand );
            return rootCommand.Invoke( args );
        }
    }
}
