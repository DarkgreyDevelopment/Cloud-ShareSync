using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static List<Tuple<BackBlazeB2Table, B2FileResponse>> CorrelateBackBlazeRecords(
            List<B2FileResponse> uploadedFileList
        ) {
            using Activity? activity = s_source.StartActivity(
                "ValidateExistingUploads.CorrelateBackBlazeRecords" )?.Start( );

            s_logger?.ILog?.Info( "Correlating existing backblaze records." );
            s_logger?.ILog?.Info( $"Received a list of {uploadedFileList.Count} existing items from backblaze." );

            SqliteContext sqliteContext = GetSqliteContext( );

            List<Tuple<BackBlazeB2Table, B2FileResponse>> result = new( );
            foreach (B2FileResponse fr in uploadedFileList) {
                BackBlazeB2Table? b2TableData = TryGetBackBlazeB2Data( fr.fileId, sqliteContext );
                if (b2TableData != null) {
                    s_logger?.ILog?.Debug( $"Matched file id '{fr.fileId}' to b2 db record id {b2TableData.Id}." );
                    result.Add( new( b2TableData, fr ) );
                }
            }

            ReleaseSqliteContext( );

            s_logger?.ILog?.Info( $"Associated {result.Count} backblaze files with previously uploaded items." );
            s_logger?.ILog?.Info( "Finished correlating existing backblaze records." );

            activity?.Stop( );
            return result;
        }

    }
}
