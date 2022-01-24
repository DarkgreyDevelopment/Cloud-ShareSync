using System.Diagnostics;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static PrimaryTable? TryGetTableDataForUpload( string path ) {
            SqliteContext sqliteContext = GetSqliteContext( );
            PrimaryTable? result = TryGetTableDataForUpload( path, sqliteContext );
            ReleaseSqliteContext( );
            return result;
        }

        private static PrimaryTable? TryGetTableDataForUpload(
            string path,
            SqliteContext sqliteContext
        ) {
            if (s_config?.SimpleBackup == null) {
                throw new InvalidDataException( "SimpleBackup config cannot be null" );
            }
            return TryGetTableDataForUpload(
                new FileInfo( path ).Name,
                Path.GetRelativePath( s_config.SimpleBackup.RootFolder, path ),
                sqliteContext
            );
        }

        private static PrimaryTable? TryGetTableDataForUpload(
            string uploadFileName,
            string uploadPath
        ) {
            SqliteContext sqliteContext = GetSqliteContext( );
            PrimaryTable? result = TryGetTableDataForUpload( uploadFileName, uploadPath, sqliteContext );
            ReleaseSqliteContext( );
            return result;
        }

        private static PrimaryTable? TryGetTableDataForUpload(
            string uploadFileName,
            string uploadPath,
            SqliteContext sqliteContext
        ) {
            using Activity? activity = s_source.StartActivity( "TryGetTableDataForUpload" )?.Start( );

            PrimaryTable? result = (from rec in sqliteContext.CoreData.AsParallel( ).AsOrdered( )
                                    where rec.FileName == uploadFileName && rec.UploadPath == uploadPath
                                    select rec).FirstOrDefault( );
            activity?.Stop( );
            return result;
        }

        private static PrimaryTable[] TryGetTableDataForUpload( long[] ids ) {
            SqliteContext sqliteContext = GetSqliteContext( );
            PrimaryTable[] result = TryGetTableDataForUpload( ids, sqliteContext );
            ReleaseSqliteContext( );
            return result;
        }

        private static PrimaryTable[] TryGetTableDataForUpload(
            long[] ids,
            SqliteContext sqliteContext
        ) {
            using Activity? activity = s_source.StartActivity( "TryGetTableDataForUpload" )?.Start( );

            PrimaryTable[] result = sqliteContext.CoreData.Where( b => ids.Contains( b.Id ) ).ToArray( );

            activity?.Stop( );
            return result;
        }

        private static PrimaryTable? TryGetTableDataForUpload( long id ) {
            SqliteContext sqliteContext = GetSqliteContext( );
            PrimaryTable? result = TryGetTableDataForUpload( id, sqliteContext );
            ReleaseSqliteContext( );
            return result;
        }

        private static PrimaryTable? TryGetTableDataForUpload(
            long id,
            SqliteContext sqliteContext
        ) {
            using Activity? activity = s_source.StartActivity( "TryGetTableDataForUpload" )?.Start( );

            PrimaryTable? result = (from rec in sqliteContext.CoreData.AsParallel( ).AsOrdered( )
                                    where rec.Id == id
                                    select rec).FirstOrDefault( );
            activity?.Stop( );
            return result;
        }

    }
}
