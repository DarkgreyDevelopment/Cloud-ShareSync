using System.Diagnostics;
using Cloud_ShareSync.Core.Compression;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static async Task UploadFileProcess( string path ) {
            using Activity? activity = s_source.StartActivity( "UploadFileProcess" )?.Start( );

            if (s_config?.SimpleBackup == null || s_config?.BackBlaze == null) {
                activity?.Stop( );
                throw new InvalidDataException( "BackBlaze/SimpleBackup configs cannot be null" );
            }
            BackupConfig config = s_config.SimpleBackup;

            // Initialize Required Variables
            FileInfo uploadFile = new( path );
            FileInfo originalUploadFile = uploadFile;
            string uploadPath = Path.GetRelativePath( config.RootFolder, path );
            string? password = config.UniqueCompressionPasswords ?
                                            UniquePassword.Create( ) :
                                            null;

            // Get Sha 512 FileHash
            string sha512filehash = await GetSha512FileHash( uploadFile );

            // Get db context
            SqliteContext sqliteContext = GetSqliteContext( );

            // Get Primary Table Data/Create new Primary Table Entry.
            PrimaryTable? tabledata = TryGetTableDataForUpload( uploadFile.Name, uploadPath, sqliteContext );
            if (tabledata == null) {
                tabledata = NewTableData( uploadFile, uploadPath, sha512filehash, sqliteContext );
            } else if (await ShouldUpload( tabledata, sha512filehash, sqliteContext ) == false) {
                s_logger?.ILog?.Info( "File already exists in backblaze. Skipping upload." );
                activity?.Stop( );
                return;
            }

            // Determine whether to copy file to the working dir for processing.
            uploadFile = (config.EncryptBeforeUpload || config.CompressBeforeUpload) ?
                CopyToWorkingDir( path ) :
                uploadFile;

            // Conditionally encrypt file before upload.
            if (config.EncryptBeforeUpload) {
                uploadFile = await EncryptFile( uploadFile, sha512filehash, tabledata, sqliteContext );
            }

            // Conditionally compress file before upload.
            if (config.CompressBeforeUpload) {
                uploadFile = CompressFile(
                    uploadFile,
                    tabledata,
                    password,
                    s_config.Compression?.CompressionCmdlineArgs,
                    sqliteContext
                );
            }

            SetUploadFileHash( originalUploadFile, uploadFile, tabledata, sha512filehash, sqliteContext );

            s_logger?.ILog?.Info( "UploadFileProcess Table Data:" );
            s_logger?.ILog?.Info( tabledata );
            // Upload File.
            await UploadFileToB2(
                uploadFile,
                path,
                uploadPath,
                sha512filehash,
                tabledata,
                s_config.BackBlaze,
                s_config.SimpleBackup,
                sqliteContext
            );
            ReleaseSqliteContext( );

            // Remove file from working directory (if needed).
            DeleteWorkingFile( uploadFile, config );

            activity?.Stop( );
        }

    }
}
