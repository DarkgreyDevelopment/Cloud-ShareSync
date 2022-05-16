using System.Diagnostics;
using Cloud_ShareSync.Core.BackgroundService.DownloadFile;
using Cloud_ShareSync.Core.BackgroundService.UploadFile;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Cloud_ShareSync.Core.CloudProvider.Types;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Database;
using Cloud_ShareSync.Core.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.BackgroundService.PrepFile {
    internal class PrepFileProcess : IPrepFileProcess {

        #region Fields

        private static readonly ActivitySource s_source = new( "PrepUploadFileProcess" );
        private readonly ILogger<PrepFileProcess> _log;
        private readonly Hashing _fileHash;
        private readonly ServiceProvider _services;
        private readonly SemaphoreSlim _semaphore = new( 0, 1 );
        private readonly B2Api _backBlaze;
        private readonly string _rootFolder;
        private List<B2File> _b2Files;
        private DateTime _lastRetrieved;

        #endregion Fields

        public PrepFileProcess(
            SyncConfig backupConfig,
            B2Config backblazeConfig,
            DatabaseConfig databaseConfig,
            ILogger<PrepFileProcess> log
        ) {
            _log = log;
            _fileHash = new( _log );
            _services = new DatabaseServices( databaseConfig.SqliteDBPath, _log ).Services;
            _ = _semaphore.Release( 1 );
            _backBlaze = new(
                backblazeConfig.ApplicationKeyId,
                backblazeConfig.ApplicationKey,
                backblazeConfig.BucketName,
                backblazeConfig.BucketId,
                backblazeConfig.MaxConsecutiveErrors,
                backblazeConfig.ProcessThreads,
                _log
            );

            _rootFolder = backupConfig.SyncFolder;
            _lastRetrieved = DateTime.Now.AddMinutes( -5 );
            _b2Files = GetB2FileList( ).Result;
        }

        #region Prep

        public Task Prep( List<string> paths ) {
            using Activity? activity = s_source.StartActivity( "Prep" )?.Start( );

            List<PrepItem> list = new( );
            foreach (string path in paths) {
                list.Add( new( path, _rootFolder ) );
            }
            _log.LogDebug(
                "Begin PrepUploadFileProcess. Created {long} Prep Items. RootFolder: {string}.",
                list.Count,
                _rootFolder
            );
            CorrelatePrimaryTableData( list );
            CorrelateBackBlazeTableData( list );
            EnqueuePrepItems( list );

            activity?.Stop( );
            return Task.CompletedTask;
        }

        private void CorrelatePrimaryTableData( List<PrepItem> list ) {
            using Activity? activity = s_source.StartActivity( "CorrelatePrimaryTableData" )?.Start( );

            List<string> fileNames = new( );
            List<string> filePaths = new( );

            foreach (PrepItem item in list) {
                fileNames.Add( item.File.Name );
                filePaths.Add( item.RelativeFilePath );
            }

            _log.LogDebug( "Retrieving Primary Table Data." );
            SqliteContext sqliteContext = GetSqliteContext( );
            PrimaryTable[] dbRecords = (
                from rec in sqliteContext.CoreData.AsParallel( ).AsOrdered( )
                where
                    fileNames.Contains( rec.FileName ) &&
                    filePaths.Contains( rec.RelativeUploadPath )
                select rec
            ).ToArray( );
            ReleaseSqliteContext( );
            _log.LogDebug( "Retrieved {int} Primary Table Data Entries.", dbRecords.Length );

            int count = 0;
            foreach (PrimaryTable rec in dbRecords) {
                PrepItem? item = list.FirstOrDefault(
                    e =>
                        e.File.Name == rec.FileName &&
                        e.RelativeFilePath == rec.RelativeUploadPath
                );

                if (item != null) {
                    item.CoreData = rec;
                    count++;
                }
            }
            _log.LogDebug( "Correlated {int} files with existing database records.", count );

            activity?.Stop( );
        }

        private void CorrelateBackBlazeTableData( List<PrepItem> list ) {
            using Activity? activity = s_source.StartActivity( "CorrelateBackBlazeTableData" )?.Start( );

            long[] ids = (
                from item in list
                where item.CoreData?.Id != null
                select item.CoreData?.Id
             ).OfType<long>( ).ToArray( );

            SqliteContext sqliteContext = GetSqliteContext( );
            BackBlazeB2Table[] dbRecords = (
                from rec in sqliteContext.BackBlazeB2Data.AsParallel( ).AsOrdered( )
                where ids.Contains( rec.Id )
                select rec
            ).ToArray( );
            ReleaseSqliteContext( );

            int count = 0;
            foreach (BackBlazeB2Table rec in dbRecords) {
                PrepItem? item = list.FirstOrDefault(
                    e =>
                        e.CoreData?.Id != null &&
                        e.CoreData.Id == rec.Id
                );

                if (item != null) {
                    item.BackBlazeData = rec;
                    count++;
                }
            }
            _log.LogDebug( "Correlated {int} files with existing backblaze records.", count );

            activity?.Stop( );
        }

        private static void EnqueuePrepItems( List<PrepItem> list ) {
            List<PrepItem> unMatched = list.Where( e => e.CoreData == null || e.BackBlazeData == null ).ToList( );

            // Add unmatched before matched.
            foreach (PrepItem item in unMatched) {
                IPrepFileProcess.Queue.Enqueue( item );
                _ = list.Remove( item );
            }
            foreach (PrepItem item in list) { IPrepFileProcess.Queue.Enqueue( item ); }
        }

        #endregion Prep

        #region Process Restore

        public Task ProcessRestore( ) {
            using Activity? activity = s_source.StartActivity( "ProcessRestore" )?.Start( );
            foreach (DownloadFileInput downloadInput in GetDatabaseRecords( )) {
                IDownloadFileProcess.Queue.Enqueue( downloadInput );
            }
            activity?.Stop( );
            return Task.CompletedTask;
        }

        public List<DownloadFileInput> GetDatabaseRecords( ) {

            SqliteContext sqliteContext = GetSqliteContext( );
            BackBlazeB2Table[] dbRecords = (
                from rec in sqliteContext.BackBlazeB2Data.AsParallel( ).AsOrdered( )
                select rec
            ).ToArray( );

            long[] b2Ids = dbRecords.Select( e => e.Id ).ToArray( );
            PrimaryTable[] primaryRecords = (
                from rec in sqliteContext.CoreData.AsParallel( ).AsOrdered( )
                where b2Ids.Contains( rec.Id )
                select rec
            ).ToArray( );

            long[] pids = primaryRecords.Select( e => e.Id ).ToArray( );

            EncryptionTable[] encRecords = (
                from rec in sqliteContext.EncryptionData.AsParallel( ).AsOrdered( )
                where pids.Contains( rec.Id )
                select rec
            ).ToArray( );

            CompressionTable[] compRecords = (
                from rec in sqliteContext.CompressionData.AsParallel( ).AsOrdered( )
                where pids.Contains( rec.Id )
                select rec
            ).ToArray( );
            ReleaseSqliteContext( );

            List<DownloadFileInput> downloadInputs = new( );
            foreach (PrimaryTable rec in primaryRecords) {
                DownloadFileInput downloadInput = new(
                    new FileInfo( Path.Join( _rootFolder, rec.RelativeUploadPath ) ),
                    rec
                ) {
                    BackBlazeData = dbRecords.First( e => e.Id == rec.Id ),
                    EncryptionData = encRecords.FirstOrDefault( e => e.Id == rec.Id ),
                    CompressionData = compRecords.FirstOrDefault( e => e.Id == rec.Id )
                };
                downloadInputs.Add( downloadInput );
            }
            return downloadInputs;
        }

        #endregion Process Restore


        #region Process Backup

        public async Task ProcessBackup( ) {
            using Activity? activity = s_source.StartActivity( "ProcessBackup" )?.Start( );
            while (IPrepFileProcess.Queue.IsEmpty == false) {
                bool deQueue = IPrepFileProcess.Queue.TryDequeue( out PrepItem? item );
                if (deQueue && item != null) {
                    UploadFileInfo upload = new( item.File, item.RelativeFilePath, _log );
                    bool newTableData = false;
                    if (item.CoreData == null) {
                        item.CoreData = NewTableData( upload.FilePath, upload.UploadFilePath );
                        newTableData = true;
                    }
                    if (await CheckShouldUpload( item, newTableData )) {
                        IUploadFileProcess.Queue.Enqueue( new( upload, item.CoreData ) );
                    }
                }
            }
            activity?.Stop( );
        }

        private PrimaryTable NewTableData( FileInfo uploadFile, string uploadPath ) {
            using Activity? activity = s_source.StartActivity( "NewTableData" )?.Start( );

            PrimaryTable result = new( ) {
                FileName = uploadFile.Name,
                RelativeUploadPath = uploadPath,
                FileHash = "",
                UploadedFileHash = "",
                IsEncrypted = false,
                IsCompressed = false,
                StoredInAwsS3 = false,
                StoredInAzureBlobStorage = false,
                StoredInBackBlazeB2 = true,
                StoredInGoogleCloudStorage = false
            };

            SqliteContext sqliteContext = GetSqliteContext( );
            _ = sqliteContext.Add( result );
            _ = sqliteContext.SaveChanges( );
            ReleaseSqliteContext( );

            activity?.Stop( );
            return result;
        }

        private async Task<bool> CheckShouldUpload( PrepItem item, bool newTableData ) {
            using Activity? activity = s_source.StartActivity( "CheckShouldUpload" )?.Start( );

            if (newTableData) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "File is new and did not have a database entry yet.",
                    item.File.FullName
                );
                return true;
            }

            // File doesn't exist in backblaze and so file should be uploaded.
            if (item.BackBlazeData == null) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "File does not have a backblaze database entry yet.",
                    item.File.FullName
                );

                activity?.Stop( );
                return true;
            }

            // Get Sha 512 FileHash
            string sha512filehash = await _fileHash.GetSha512Hash( item.File );

            // Sha 512 hashes are different and so file should be uploaded.
            if (item.CoreData?.FileHash != sha512filehash) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "Database file hash does not match local file hash.",
                    item.File.FullName
                );
                _log.LogInformation(
                    "\nsha512filehash: {string}\n" +
                    "DBFileHash:     {string}.",
                    sha512filehash, item.CoreData?.FileHash
                );

                activity?.Stop( );
                return true;
            }

            _log.LogInformation(
                "File has an existing backblaze database record. Database sha512 hash matches current filehash."
            );

            _log.LogInformation(
                "Querying backblaze to validate uploaded filehash matches database/local filehash."
            );

            List<B2File> fileResponse = await GetB2FileList( );

            B2File? b2Resp = fileResponse.FirstOrDefault( e => e.fileId == item.BackBlazeData.FileID );

            if (b2Resp == null) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "BackBlaze did not return anything for the specified fileid.",
                    item.File.FullName
                );

                activity?.Stop( );
                return true;
            }

            Dictionary<string, string> fileInfo = b2Resp.fileInfo;

            if (fileInfo.ContainsKey( "sha512_filehash" ) == false) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "File response from BackBlaze is missing required Sha512 filehash metadata.",
                    item.File.FullName
                );

                activity?.Stop( );
                return true;
            }

            if (fileInfo["sha512_filehash"] != sha512filehash) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "Local file and uploaded file are different.",
                    item.File.FullName
                );
                _log.LogInformation(
                    "\nFFileHash: {string}\n" +
                    "FileHash : {string}",
                    fileInfo["sha512_filehash"],
                    sha512filehash
                );
                foreach (string key in fileInfo.Keys) {
                    string msg = $"{key}: {fileInfo[key]}";
                    _log.LogInformation( "{string}", msg );
                }

                activity?.Stop( );
                return true;
            }

            _log.LogInformation(
                "'{string}' does not need to be uploaded to backblaze. " +
                "Uploaded file hash matches local file hash.",
                item.File.FullName
            );
            activity?.Stop( );
            return false;
        }

        #endregion Process Backup

        private async Task<List<B2File>> GetB2FileList( ) {
            using Activity? activity = s_source.StartActivity( "GetB2FileList" )?.Start( );
            if (_lastRetrieved < DateTime.Now.AddMinutes( -1 )) {
                _b2Files = await _backBlaze.ListBucketFiles( );
                _lastRetrieved = DateTime.Now;
            }
            activity?.Stop( );
            return _b2Files;
        }

        private SqliteContext GetSqliteContext( ) {
            using Activity? activity = s_source.StartActivity( "GetSqliteContext" )?.Start( );

            _semaphore.Wait( );
            SqliteContext result = _services.GetRequiredService<SqliteContext>( );

            activity?.Stop( );
            return result;
        }

        private void ReleaseSqliteContext( ) { _ = _semaphore.Release( ); }

    }
}
