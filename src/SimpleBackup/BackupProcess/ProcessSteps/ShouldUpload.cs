using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static async Task<bool> ShouldUpload(
            PrimaryTable tabledata,
            string sha512filehash,
            SqliteContext sqliteContext
        ) {

            if (s_backBlaze == null) {
                throw new InvalidOperationException( "Cannot proceed if backblaze configuration is not initialized." );
            }

            using Activity? activity = s_source.StartActivity( "ShouldUpload" )?.Start( );
            BackBlazeB2Table? b2TableData = TryGetBackBlazeB2Data( tabledata.Id, sqliteContext );

            if (b2TableData != null && tabledata.FileHash == sha512filehash) {
                s_logger?.ILog?.Info( "File has an existing backblaze database record. Previous sha512 matches current filehash." );
                string filename = string.IsNullOrWhiteSpace( tabledata.UploadPath ) ?
                    tabledata.FileName :
                    tabledata.UploadPath;

                s_logger?.ILog?.Info( "Querying backblaze to validate filehash." );
                List<B2FileResponse> fileResponse = await s_backBlaze.ListFileVersions(
                    filename,
                    b2TableData.FileID,
                    true
                );
                s_logger?.ILog?.Info( "ShouldUpload db data:" );
                s_logger?.ILog?.Info( tabledata );
                s_logger?.ILog?.Info( b2TableData );

                if (
                    fileResponse.Count > 0 &&
                    fileResponse[0].fileInfo.ContainsKey( "sha512_filehash" ) &&
                    fileResponse[0].fileInfo["sha512_filehash"] == sha512filehash
                ) {
                    s_logger?.ILog?.Info( "Backblaze and local Sha512 file hashes match. Skipping file upload." );
                    activity?.Stop( );
                    return false;
                } else {
                    s_logger?.ILog?.Info( "Backblaze and local Sha512 file hashes DO NOT match." );
                }
            }

            s_logger?.ILog?.Info( "File should be uploaded to backblaze." );

            activity?.Stop( );
            return true;
        }

    }
}
