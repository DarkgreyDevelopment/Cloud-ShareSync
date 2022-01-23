using System.Diagnostics;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static PrimaryTable? TryGetTableDataForUpload( string path, SqliteContext sqliteContext ) {
            if (s_config?.SimpleBackup == null || s_config?.BackBlaze == null) {
                throw new InvalidDataException( "SimpleBackup and BackBlaze configs cannot be null" );
            }
            return TryGetTableDataForUpload(
                new FileInfo( path ).Name,
                Path.GetRelativePath( s_config.SimpleBackup.RootFolder, path ),
                sqliteContext
            );
        }

        private static PrimaryTable? TryGetTableDataForUpload(
            string uploadFileName,
            string uploadPath,
            SqliteContext sqliteContext
        ) {
            using Activity? activity = s_source.StartActivity( "TryGetTableDataForUpload" )?.Start( );

            PrimaryTable? result = (from b in sqliteContext.CoreData.AsParallel( ).AsOrdered( )
                                    where b.FileName == uploadFileName && b.UploadPath == uploadPath
                                    select b).FirstOrDefault( );
            activity?.Stop( );
            return result;
        }

    }
}
