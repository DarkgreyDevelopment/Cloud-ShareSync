using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Database;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.SharedServices.BackgroundService.Interfaces;
using Cloud_ShareSync.Core.SharedServices.BackgroundService.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.SharedServices.BackgroundService.Process {
    internal class PrepUploadFileProcess : IPrepUploadFileProcess {

        #region Fields

        private static readonly ActivitySource s_source = new( "PrepUploadFileProcess" );
        private static readonly ConcurrentQueue<PrepItem> s_queue = new( );
        private readonly ILogger<PrepUploadFileProcess> _log;
        private readonly Hashing _fileHash;
        private readonly CloudShareSyncServices _services;
        private readonly SemaphoreSlim _semaphore = new( 0, 1 );
        private readonly BackBlazeB2 _backBlaze;
        private readonly string _rootFolder;
        private List<B2FileResponse> _b2FileResponses;
        private DateTime _lastRetrieved;

        #endregion Fields

        public PrepUploadFileProcess(
            SyncConfig backupConfig,
            B2Config backblazeConfig,
            DatabaseConfig databaseConfig,
            ILogger<PrepUploadFileProcess> log
        ) {
            _log = log;
            _fileHash = new( _log );
            _services = ConfigManager.ConfigureDatabaseService( databaseConfig, _log );
            _ = _semaphore.Release( 1 );
            _backBlaze = new( backblazeConfig, _log );
            _rootFolder = backupConfig.SyncFolder;
            _lastRetrieved = DateTime.Now.AddMinutes( -5 );
            _b2FileResponses = GetB2FileResponseList( ).Result;
        }

        public Task Prep( List<string> paths ) {
            using Activity? activity = s_source.StartActivity( "Prep" )?.Start( );

            List<PrepItem> list = new( );
            foreach (string path in paths) {
                list.Add( new( path, _rootFolder ) );
            }
            CorrelatePrimaryTableData( list );
            CorrelateBackBlazeTableData( list );

            List<PrepItem> unMatched = list.Where( e => e.CoreData == null || e.BackBlazeData == null ).ToList( );

            // Add unmatched before matched.
            foreach (PrepItem item in unMatched) {
                s_queue.Enqueue( item );
                _ = list.Remove( item );
            }
            foreach (PrepItem item in list) { s_queue.Enqueue( item ); }

            activity?.Stop( );
            return Task.CompletedTask;
        }

        public async Task Process( ) {
            using Activity? activity = s_source.StartActivity( "Process" )?.Start( );

            while (s_queue.IsEmpty == false) {
                bool deQueue = s_queue.TryDequeue( out PrepItem? item );
                if (deQueue && item != null) {
                    bool newTableData = false;
                    if (item.CoreData == null) {
                        item.CoreData = NewTableData( item.UploadFile, item.UploadPath );
                        newTableData = true;
                    }

                    if (await CheckShouldUpload( item, newTableData )) {
                        IUploadFileProcess.Queue.Enqueue(
                            new( item.UploadFile, item.UploadPath, item.CoreData )
                        );
                    }
                }
            }

            activity?.Stop( );
        }

        #region PrivateMethods

        private void CorrelatePrimaryTableData( List<PrepItem> list ) {
            using Activity? activity = s_source.StartActivity( "CorrelatePrimaryTableData" )?.Start( );

            List<string> uploadFileNames = new( );
            List<string> uploadPaths = new( );

            foreach (PrepItem item in list) {
                uploadFileNames.Add( item.UploadFile.Name );
                uploadPaths.Add( item.UploadPath );
            }

            SqliteContext sqliteContext = GetSqliteContext( );
            PrimaryTable[] dbRecords = (
                from rec in sqliteContext.CoreData.AsParallel( ).AsOrdered( )
                where
                    uploadFileNames.Contains( rec.FileName ) &&
                    uploadPaths.Contains( rec.RelativeUploadPath )
                select rec
            ).ToArray( );
            ReleaseSqliteContext( );

            int count = 0;
            foreach (PrimaryTable rec in dbRecords) {
                PrepItem? item = list.FirstOrDefault(
                    e =>
                        e.UploadFile.Name == rec.FileName &&
                        e.UploadPath == rec.RelativeUploadPath
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

        private async Task<bool> CheckShouldUpload( PrepItem item, bool newTableData ) {
            using Activity? activity = s_source.StartActivity( "CheckShouldUpload" )?.Start( );

            if (newTableData) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "File is new and did not have a database entry yet.",
                    item.UploadFile.FullName
                );
                return true;
            }

            // File doesn't exist in backblaze and so file should be uploaded.
            if (item.BackBlazeData == null) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "File does not have a backblaze database entry yet.",
                    item.UploadFile.FullName
                );

                activity?.Stop( );
                return true;
            }

            // Get Sha 512 FileHash
            string sha512filehash = await _fileHash.GetSha512Hash( item.UploadFile );

            // Sha 512 hashes are different and so file should be uploaded.
            if (item.CoreData?.FileHash != sha512filehash) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "Database file hash does not match local file hash.",
                    item.UploadFile.FullName
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

            List<B2FileResponse> fileResponse = await GetB2FileResponseList( );

            string startFileName = string.IsNullOrWhiteSpace( item.CoreData.RelativeUploadPath ) ?
                    item.CoreData.FileName : item.CoreData.RelativeUploadPath;

            B2FileResponse? b2Resp = fileResponse.FirstOrDefault( e => e.fileId == item.BackBlazeData.FileID );

            if (b2Resp == null) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "BackBlaze did not return anything for the specified fileid.",
                    item.UploadFile.FullName
                );

                activity?.Stop( );
                return true;
            }

            Dictionary<string, string> fileInfo = b2Resp.fileInfo;

            if (fileInfo.ContainsKey( "sha512_filehash" ) == false) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "File response from BackBlaze is missing required Sha512 filehash metadata.",
                    item.UploadFile.FullName
                );

                activity?.Stop( );
                return true;
            }

            if (fileInfo["sha512_filehash"] != sha512filehash) {
                _log.LogInformation(
                    "'{string}' should be uploaded to backblaze. " +
                    "Local file and uploaded file are different.",
                    item.UploadFile.FullName
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
                item.UploadFile.FullName
            );
            activity?.Stop( );
            return false;
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

        private async Task<List<B2FileResponse>> GetB2FileResponseList( ) {
            using Activity? activity = s_source.StartActivity( "GetB2FileResponseList" )?.Start( );
            if (_lastRetrieved < DateTime.Now.AddMinutes( -1 )) {
                _b2FileResponses = await _backBlaze.ListFileVersions( );
                _lastRetrieved = DateTime.Now;
            }
            activity?.Stop( );
            return _b2FileResponses;
        }

        private SqliteContext GetSqliteContext( ) {
            using Activity? activity = s_source.StartActivity( "GetSqliteContext" )?.Start( );

            _semaphore.Wait( );
            SqliteContext result = _services.Services.GetRequiredService<SqliteContext>( );

            activity?.Stop( );
            return result;
        }

        private void ReleaseSqliteContext( ) { _ = _semaphore.Release( ); }

        #endregion PrivateMethods
    }
}
