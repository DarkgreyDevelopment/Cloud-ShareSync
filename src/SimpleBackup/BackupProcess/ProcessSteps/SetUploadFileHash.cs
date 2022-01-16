using System.Diagnostics;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static async void SetUploadFileHash(
            FileInfo originalUploadFile,
            FileInfo uploadFile,
            PrimaryTable tabledata,
            string sha512filehash
        ) {
            using Activity? activity = s_source.StartActivity( "SetUploadFileHash" )?.Start( );

            if (uploadFile == originalUploadFile) {
                tabledata.UploadedFileHash = sha512filehash;
            } else {
                s_logger?.ILog?.Info( "Upload file has been compressed or encrypted." );
                tabledata.UploadedFileHash = await GetSha512FileHash( uploadFile );
            }
            s_sqlliteContext?.SaveChanges( );

            activity?.Stop( );
        }

    }
}
