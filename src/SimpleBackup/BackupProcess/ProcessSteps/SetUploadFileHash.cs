using System.Diagnostics;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static async void SetUploadFileHash(
            FileInfo originalUploadFile,
            FileInfo uploadFile,
            PrimaryTable tabledata,
            string sha512filehash,
            SqliteContext sqliteContext
        ) {
            using Activity? activity = s_source.StartActivity( "SetUploadFileHash" )?.Start( );

            if (uploadFile == originalUploadFile) {
                tabledata.UploadedFileHash = sha512filehash;
            } else {
                s_logger?.ILog?.Info( "Upload file has been compressed or encrypted." );
                tabledata.UploadedFileHash = await GetSha512FileHash( uploadFile );
            }
            sqliteContext.SaveChanges( );

            activity?.Stop( );
        }

    }
}
