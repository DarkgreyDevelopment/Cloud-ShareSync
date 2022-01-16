using System.Diagnostics;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static PrimaryTable NewTableData(
            FileInfo uploadFile,
            string uploadPath,
            string sha512filehash
        ) {
            using Activity? activity = s_source.StartActivity( "NewTableData" )?.Start( );

            s_sqlliteContext?.Add(
                new PrimaryTable {
                    FileName = uploadFile.Name,
                    UploadPath = uploadPath,
                    FileHash = sha512filehash,
                    UploadedFileHash = "",
                    IsEncrypted = false,
                    IsCompressed = false,
                    UsesAwsS3 = false,
                    UsesAzureBlobStorage = false,
                    UsesBackBlazeB2 = true,
                    UsesGoogleCloudStorage = false
                }
            );
            s_sqlliteContext?.SaveChanges( );
            PrimaryTable? tabledata = s_sqlliteContext?.CoreData
                .Where( b => (b.FileName == uploadFile.Name && b.UploadPath == uploadPath) )
                .FirstOrDefault( ) ??
                throw new InvalidOperationException( "PrimaryTable item was not created." );

            activity?.Stop( );
            return tabledata;
        }

    }
}
