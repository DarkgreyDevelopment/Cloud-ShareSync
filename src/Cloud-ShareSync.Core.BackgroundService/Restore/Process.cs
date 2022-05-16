using System.Diagnostics;
using Cloud_ShareSync.Core.BackgroundService.DownloadFile;
using Cloud_ShareSync.Core.BackgroundService.PrepFile;
using Cloud_ShareSync.Core.Configuration.ManagedActions;
using Cloud_ShareSync.Core.Configuration.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.BackgroundService.Restore {

    public class Process {
        private static readonly ActivitySource s_source = new( "BackgroundService.Restore" );

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
        }

        #endregion Constructor and Inititalization.

        public async Task Run( ) {
            using Activity? activity = s_source.StartActivity( "Run" )?.Start( );
            try {
                await PrepProcess( );
                Task[] downloadTasks = StartDownloadTasks( );
                await WaitOnTasks( downloadTasks );
                if (IDownloadFileProcess.Queue.IsEmpty) {
                    _log?.LogInformation( "Restore completed successfully." );
                } else {
                    _log?.LogInformation( "Restore process has not completed successfully." );
                    _log?.LogInformation( "Download Task Status:" );
                    WriteTaskStatusInfo( downloadTasks );
                }
            } catch (Exception e) {
                WriteException( e );
            }
            activity?.Stop( );
        }

        private async Task PrepProcess( ) {
            _log?.LogDebug( "Begin Prep Process." );
            IPrepFileProcess prepWorker = _host.Services.GetRequiredService<IPrepFileProcess>( );
            await prepWorker.ProcessRestore( );
            _log?.LogDebug( "Completed Prep Process." );
        }

        private Task[] StartDownloadTasks( ) {
            _log?.LogInformation( "Kicking off download process tasks." );
            Task[] downloadTasks = new Task[2];
            for (int i = 0; i < downloadTasks.Length; i++) {
                downloadTasks[i] = DownloadWork( );
            }
            return downloadTasks;
        }

        private async Task DownloadWork( ) {
            _log?.LogDebug( "Begin Download Work." );
            IDownloadFileProcess downloadWorker = _host.Services.GetRequiredService<IDownloadFileProcess>( );
            await downloadWorker.Process( );
            _log?.LogDebug( "Completed Download Work." );
        }

        private async Task WaitOnTasks( Task[] uploadTasks ) {
            while (uploadTasks.Any( e => e.IsCompleted != true )) {
                await Task.Delay( 2500 );
            }
        }

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

        private void WriteException( Exception e ) {
            if (_log == null) {
                Console.WriteLine( e.ToString( ) );
            } else {
                _log.LogCritical( "{exception}", e );
            }
        }

    }
}
