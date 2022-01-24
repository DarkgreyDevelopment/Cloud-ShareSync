using System.Diagnostics;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static BackBlazeB2Table[] TryGetBackBlazeB2Data( string[] fileIds, SqliteContext sqliteContext ) {
            using Activity? activity = s_source.StartActivity( "TryGetBackBlazeB2Data" )?.Start( );

            BackBlazeB2Table[] result = sqliteContext.BackBlazeB2Data.Where( b => fileIds.Contains( b.FileID ) ).ToArray( );

            activity?.Stop( );
            return result;
        }

        private static BackBlazeB2Table? TryGetBackBlazeB2Data( string fileId, SqliteContext sqliteContext ) {
            using Activity? activity = s_source.StartActivity( "TryGetBackBlazeB2Data" )?.Start( );

            BackBlazeB2Table? result = string.IsNullOrWhiteSpace( fileId ) ?
                                        null :
                                        sqliteContext.BackBlazeB2Data
                                            .Where( b => b.FileID == fileId )
                                            .FirstOrDefault( );

            activity?.Stop( );
            return result;
        }

        private static BackBlazeB2Table? TryGetBackBlazeB2Data( long? id, SqliteContext sqliteContext ) {
            using Activity? activity = s_source.StartActivity( "TryGetBackBlazeB2Data" )?.Start( );

            BackBlazeB2Table? result = id == null ?
                                        null :
                                        sqliteContext.BackBlazeB2Data
                                            .Where( b => b.Id == id )
                                            .FirstOrDefault( );
            activity?.Stop( );
            return result;
        }

        private static BackBlazeB2Table? TryGetBackBlazeB2Data( long? id ) {
            using Activity? activity = s_source.StartActivity( "TryGetBackBlazeB2Data" )?.Start( );

            SqliteContext sqliteContext = GetSqliteContext( );
            BackBlazeB2Table? result = TryGetBackBlazeB2Data( id, sqliteContext );
            ReleaseSqliteContext( );

            activity?.Stop( );
            return result;
        }

    }
}
