using System.Diagnostics;
using System.Security.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption.Types;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static async Task<FileInfo> EncryptFile(
            FileInfo uploadFile,
            string sha512filehash,
            PrimaryTable tabledata,
            SqliteContext sqliteContext
        ) {
            using Activity? activity = s_source.StartActivity( "EncryptFile" )?.Start( );
            s_logger?.ILog?.Info( "Encrypting file before upload." );

            if (s_crypto == null) { s_crypto = new( s_logger ); }

            FileInfo cypherTxtFile = new( Path.Join( s_config?.SimpleBackup?.WorkingDirectory, sha512filehash ) );

            byte[] key = RandomNumberGenerator.GetBytes( 32 );
            DecryptionData data = await s_crypto.Encrypt( key, uploadFile, cypherTxtFile, null );

            EncryptionTable? encTableData = sqliteContext.EncryptionData
                .Where( b => b.Id == tabledata.Id )
                .FirstOrDefault( );

            if (encTableData == null) {
                sqliteContext.Add( new EncryptionTable( tabledata.Id, data ) );
            } else {
                encTableData.DecryptionData = data.ToString( );
            }
            tabledata.IsEncrypted = true;
            sqliteContext.SaveChanges( );

            // Remove plaintext file.
            uploadFile.Delete( );

            activity?.Stop( );
            return cypherTxtFile;
        }

    }
}
