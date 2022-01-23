using System.Diagnostics;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static PrimaryTable NewTableData(
            FileInfo uploadFile,
            string uploadPath,
            string sha512filehash,
            SqliteContext sqliteContext
        ) {
            using Activity? activity = s_source.StartActivity( "NewTableData" )?.Start( );

            sqliteContext.Add(
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
            sqliteContext.SaveChanges( );
            PrimaryTable? tabledata = sqliteContext.CoreData
                .Where( b => (b.FileName == uploadFile.Name && b.UploadPath == uploadPath) )
                .FirstOrDefault( ) ??
                throw new InvalidOperationException( "PrimaryTable item was not created." );

            activity?.Stop( );
            return tabledata;
        }

    }
}
