using System.Diagnostics;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static PrimaryTable? TryGetTableDataForUpload( string path ) {
            if (s_config?.SimpleBackup == null || s_config?.BackBlaze == null) {
                throw new InvalidDataException( "SimpleBackup and BackBlaze configs cannot be null" );
            }
            return TryGetTableDataForUpload(
                new FileInfo( path ).Name,
                Path.GetRelativePath( s_config.SimpleBackup.RootFolder, path )
            );
        }

        private static PrimaryTable? TryGetTableDataForUpload(
            string uploadFileName,
            string uploadPath
        ) {
            using Activity? activity = s_source.StartActivity( "TryGetTableDataForUpload" )?.Start( );

            PrimaryTable? result = s_sqlliteContext?.CoreData
                .Where( b => (b.FileName == uploadFileName && b.UploadPath == uploadPath) )
                .FirstOrDefault( );

            activity?.Stop( );
            return result;
        }

    }
}
