using System.Diagnostics;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static FileInfo CompressFile(
            FileInfo uploadFile,
            PrimaryTable tabledata,
            string? password,
            string? decompressionargs
        ) {
            using Activity? activity = s_source.StartActivity( "CompressFile" )?.Start( );
            s_logger?.ILog?.Info( "Compressing file before upload." );

            FileInfo? compressedFile = CompressionInterface.CompressPath(
                uploadFile,
                password
            );

            CompressionTable? compTableData = s_sqlliteContext?.CompressionData
                .Where( b => b.Id == tabledata.Id )
                .FirstOrDefault( );

            if (compTableData == null) {
                s_sqlliteContext?.Add(
                    new CompressionTable(
                        id: tabledata.Id,
                        passwordProtected: string.IsNullOrWhiteSpace( password ) == false,
                        password: password,
                        specialDecompress: string.IsNullOrWhiteSpace( decompressionargs ) == false,
                        decompressionArgs: decompressionargs
                    ) );
            } else {
                compTableData.PasswordProtected = string.IsNullOrWhiteSpace( password ) == false;
                compTableData.Password = password;
                compTableData.SpecialDecompress = string.IsNullOrWhiteSpace( decompressionargs ) == false;
                compTableData.DecompressionArgs = decompressionargs;
            }
            tabledata.IsCompressed = true;
            s_sqlliteContext?.SaveChanges( );

            // Remove plaintext file.
            uploadFile.Delete( );

            activity?.Stop( );
            return compressedFile;
        }

    }
}
