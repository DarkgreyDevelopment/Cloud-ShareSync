using System.Diagnostics;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.SharedServices.BackgroundService;
using Cloud_ShareSync.Core.SharedServices.BackgroundService.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Backup {

    internal class Process {
        private static readonly ActivitySource s_source = new( "Cloud-ShareSync.Backup.Program" );

        internal Regex[] _excludedPathPatterns;
        internal ILogger? _log;
        internal IHost _host;
        internal SyncConfig _config;

        #region Constructor and Inititalization.

        public Process( ) {
            ConfigManager cfgMgr = new( );
            _log = ConfigManager.CreateTelemetryLogger( cfgMgr.Config.Logging );
            _host = HostProvider.ConfigureHost( _log, cfgMgr );
            _config = cfgMgr.Config.Sync;
            _excludedPathPatterns = BuildExcludeRegexArray( ); ;
        }

        private Regex[] BuildExcludeRegexArray( ) {
            using Activity? activity = s_source.StartActivity( "BuildExcludeRegexArray" )?.Start( );

            List<Regex> regexPatterns = new( );
            foreach (string exPath in _config.ExcludePaths) {
                regexPatterns.Add( new( exPath, RegexOptions.Compiled ) );
            }

            activity?.Stop( );
            return regexPatterns.ToArray( );
        }

        #endregion Constructor and Inititalization.

        internal async Task Run( ) {
            using Activity? activity = s_source.StartActivity( "Run" )?.Start( );
            try {
                List<string> fileList = PopulateFileList( );
                await RunInitialBackupProcess( fileList );
            } catch (Exception e) {
                WriteException( e );
            }
            activity?.Stop( );
        }

        #region Populate File List

        private List<string> PopulateFileList( ) {
            using Activity? activity = s_source.StartActivity( "PopulateFileList" )?.Start( );

            IEnumerable<string> files = EnumerateRootFolder( );
            List<string> fileList = BuildFileUploadQueue( files );
            _log?.LogInformation( "File upload queue contains {int} files.", fileList.Count );

            activity?.Stop( );
            return fileList;
        }

        private IEnumerable<string> EnumerateRootFolder( ) {
            using Activity? activity = s_source.StartActivity( "EnumerateRootFolder" )?.Start( );

            IEnumerable<string> files = Directory.EnumerateFiles(
                _config.SyncFolder,
                "*",
                _config.Recurse ?
                    SearchOption.AllDirectories :
                    SearchOption.TopDirectoryOnly
            );
            _log?.LogInformation( "Discovered {int} files under '{string}'.", files.Count( ), _config.SyncFolder );

            activity?.Stop( );
            return files;
        }

        private List<string> BuildFileUploadQueue( IEnumerable<string> files ) {
            using Activity? activity = s_source.StartActivity( "BuildFileUploadQueue" )?.Start( );

            List<string> fileList = new( );
            foreach (string path in files) {
                bool excludePath = ExcludePath( path );

                if (excludePath) {
                    _log?.LogDebug( "Skipping excluded file: '{string}'", path );
                } else {
                    fileList.Add( path );
                }
            }

            activity?.Stop( );
            return fileList;
        }

        private bool ExcludePath( string path ) {
            using Activity? activity = s_source.StartActivity( "ExcludePath" )?.Start( );

            bool result = false;
            foreach (Regex pattern in _excludedPathPatterns) {
                if (pattern.Match( path ).Success) {
                    result = true;
                    break;
                }
            }

            activity?.Stop( );
            return result;
        }

        #endregion Populate File List

        #region Simple Backup Process

        private async Task RunInitialBackupProcess( List<string> fileList ) {
            await PrepWork( fileList );
            Task[] prepTasks = StartPrepTasks( );
            Task[] uploadTasks = StartUploadTasks( );
            WaitOnUploadTasks( prepTasks, uploadTasks );
            ValidateQueueComplete( uploadTasks );
            if (IUploadFileProcess.Queue.IsEmpty) {
                _log?.LogInformation( "Simple backup completed successfully." );
            } else {
                _log?.LogInformation( "Simple backup process has not completed successfully." );
            }
        }

        private async Task PrepWork( List<string> fileList ) {
            _log?.LogDebug( "Begin Prep Work." );
            IPrepUploadFileProcess prepWorker = _host.Services.GetRequiredService<IPrepUploadFileProcess>( );
            await prepWorker.Prep( fileList );
            _log?.LogDebug( "Completed Prep Work." );
        }

        private Task[] StartPrepTasks( ) {
            _log?.LogInformation( "Kicking off prep process tasks." );
            Task[] prepTasks = new Task[5];
            for (int i = 0; i < prepTasks.Length; i++) {
                prepTasks[i] = PrepProcess( );
            }
            return prepTasks;
        }

        private async Task PrepProcess( ) {
            _log?.LogDebug( "Begin Prep Process." );
            IPrepUploadFileProcess prepWorker = _host.Services.GetRequiredService<IPrepUploadFileProcess>( );
            await prepWorker.Process( );
            _log?.LogDebug( "Completed Prep Process." );
        }

        private Task[] StartUploadTasks( ) {
            _log?.LogInformation( "Kicking off upload process tasks." );
            Task[] uploadTasks = new Task[2];
            StartUploadTasks( uploadTasks );
            return uploadTasks;
        }

        private async Task UploadWork( ) {
            _log?.LogDebug( "Begin Upload Work." );
            IUploadFileProcess uploadWorker = _host.Services.GetRequiredService<IUploadFileProcess>( );
            await uploadWorker.Process( );
            _log?.LogDebug( "Completed Upload Work." );
        }

        private void StartUploadTasks( Task[] uploadTasks ) {
            for (int i = 0; i < uploadTasks.Length; i++) {
                uploadTasks[i] = UploadWork( );
            }
        }

        private void WaitOnUploadTasks(
            Task[] prepTasks,
            Task[] uploadTasks
        ) {
            while (TasksComplete( prepTasks, uploadTasks )) {
                Thread.Sleep( 1000 );
                if (UploadsComplete( uploadTasks )) {
                    _log?.LogInformation( "Restarting upload process tasks." );
                    for (int i = 0; i < uploadTasks.Length; i++) {
                        uploadTasks[i] = UploadWork( );
                    }
                }
            }
        }

        private static bool TasksComplete(
            Task[] prepTasks,
            Task[] uploadTasks
        ) =>
            prepTasks.Any( e => e.IsCompleted != true ) ||
            uploadTasks.Any( e => e.IsCompleted != true );

        private static bool UploadsComplete( Task[] uploadTasks ) =>
            uploadTasks.All( e => e.IsCompleted == true ) &&
            IUploadFileProcess.Queue.IsEmpty == false;

        private void ValidateQueueComplete( Task[] uploadTasks ) {
            CheckUploadsComplete( uploadTasks, true );
            while (uploadTasks.All( e => e.IsCompleted == false )) { Thread.Sleep( 1000 ); }
            CheckUploadsComplete( uploadTasks, false );
        }

        private void CheckUploadsComplete( Task[] uploadTasks, bool restart ) {
            if (UploadsComplete( uploadTasks ) == false) {
                string message = "Upload process tasks not completed or queue is not empty. " + (
                    restart ? "Restarting upload process tasks." : "Writing task statuses."
                );
                _log?.LogInformation( "{string}", message );
                WriteTaskStatusInfo( uploadTasks );
                if (restart) { StartUploadTasks( uploadTasks ); }
            }
        }

        private void WriteTaskStatusInfo( Task[] uploadTasks ) {
            int count = 0;
            if (_log != null) {
                foreach (Task task in uploadTasks) {
                    _log.LogInformation(
                        "Task{int} Status:\n" +
                        "Completed: {bool}\n" +
                        "Cancelled: {bool}\n" +
                        "Faulted  : {bool}\n",
                        count,
                        task.IsCompleted,
                        task.IsCanceled,
                        task.IsFaulted
                    );

                    WriteTaskException( task, count );
                    count++;
                }
            }
        }

        private void WriteTaskException( Task task, int count ) {
            if (task.Exception != null && _log != null) {
                _log.LogInformation( "An error has occurred in task{string}.", count );
                _log.LogError( "{string}\n{string}", task.Exception.Message, task.Exception.StackTrace );
                foreach (Exception ex in task.Exception.InnerExceptions) {
                    _log.LogInformation( "Task{string} inner exception:", count );
                    _log.LogError( "{string}\n{string}", ex.Message, ex.StackTrace );
                }
            }
        }

        #endregion Simple Backup Process

        private void WriteException( Exception e ) {
            if (_log == null) {
                Console.WriteLine( e.ToString( ) );
            } else {
                _log.LogCritical( "{exception}", e );
            }
        }

    }
}
