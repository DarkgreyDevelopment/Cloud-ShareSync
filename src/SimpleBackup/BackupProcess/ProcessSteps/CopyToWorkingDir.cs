using System.Diagnostics;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static FileInfo CopyToWorkingDir( string path ) {
            using Activity? activity = s_source.StartActivity( "CopyToWorkingDir" )?.Start( );

            if (s_config?.SimpleBackup == null) { throw new InvalidDataException( "SimpleBackup config cannot be null" ); }
            string filePath = Path.Join( s_config.SimpleBackup.WorkingDirectory, new FileInfo( path ).Name );
            s_logger?.ILog?.Info( $"Copying '{path}' to '{filePath}'." );
            File.Copy( path, filePath, true );

            activity?.Stop( );
            return new FileInfo( filePath );
        }

    }
}
