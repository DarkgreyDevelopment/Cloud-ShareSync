using System.Diagnostics;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.BackgroundService.PrepFile;
using Cloud_ShareSync.Core.BackgroundService.UploadFile;
using Cloud_ShareSync.Core.Configuration.ManagedActions;
using Cloud_ShareSync.Core.Configuration.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.BackgroundService.Backup {

    public class Process {
        private static readonly ActivitySource s_source = new( "BackgroundService.Backup" );

        internal Regex[] _excludedPathPatterns;
        internal ILogger? _log;
        internal IHost _host;
        internal SyncConfig _config;

        #region Constructor and Inititalization.

        public Process( ) {
            ConfigManager cfgMgr = new( );
            _log = ConfigToCloudShareSyncObjectConverter
                .ConvertLog4NetConfigToILogger(
                    cfgMgr.Config.Logging,
                    Array.Empty<string>( )
                );
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

        public async Task Run( ) {
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
                if (ExcludePath( path )) {
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
            await WaitOnTasks( prepTasks, uploadTasks );
            if (IUploadFileProcess.Queue.IsEmpty) {
                _log?.LogInformation( "Backup completed successfully." );
            } else {
                _log?.LogInformation( "Backup process has not completed successfully." );
                _log?.LogInformation( "Prep Task Status:" );
                WriteTaskStatusInfo( prepTasks );

                _log?.LogInformation( "Upload Task Status:" );
                WriteTaskStatusInfo( uploadTasks );
            }
        }

        private async Task PrepWork( List<string> fileList ) {
            IPrepFileProcess prepWorker = _host.Services.GetRequiredService<IPrepFileProcess>( );
            await prepWorker.Prep( fileList );
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
            IPrepFileProcess prepWorker = _host.Services.GetRequiredService<IPrepFileProcess>( );
            await prepWorker.ProcessBackup( );
            _log?.LogDebug( "Completed Prep Process." );
        }

        private Task[] StartUploadTasks( ) {
            _log?.LogInformation( "Kicking off upload process tasks." );
            Task[] uploadTasks = new Task[2];
            StartUploadTasks( uploadTasks );
            return uploadTasks;
        }

        private void StartUploadTasks( Task[] uploadTasks ) {
            for (int i = 0; i < uploadTasks.Length; i++) {
                uploadTasks[i] = UploadWork( );
            }
        }

        private async Task UploadWork( ) {
            _log?.LogDebug( "Begin Upload Work." );
            IUploadFileProcess uploadWorker = _host.Services.GetRequiredService<IUploadFileProcess>( );
            await uploadWorker.Process( );
            _log?.LogDebug( "Completed Upload Work." );
        }

        private async Task WaitOnTasks( Task[] prepTasks, Task[] uploadTasks ) {
            while (TasksIncompleteQueuePopulated( prepTasks, uploadTasks )) {
                if (UploadTasksCompleted( uploadTasks )) {
                    _log?.LogInformation( "Restarting upload process tasks." );
                    StartUploadTasks( uploadTasks );
                }
                await Task.Delay( 2500 );
            }
        }

        private static bool UploadTasksCompleted( Task[] uploadTasks ) =>
            uploadTasks.All( e => e.IsCompleted == true );

        private static bool TasksIncompleteQueuePopulated( Task[] prepTasks, Task[] uploadTasks ) =>
            prepTasks.Any( e => e.IsCompleted == false ) ||
            UploadTasksCompleted( uploadTasks ) == false ||
            IUploadFileProcess.Queue.IsEmpty == false;

        private void WriteTaskStatusInfo( Task[] tasks ) {
            int count = 0;
            if (_log != null) {
                foreach (Task task in tasks) {
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
