using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private void DetermineMultiPartUploadSuccessStatus(
            List<Task<bool>> tasks,
            ConcurrentStack<FilePartInfo> filePartQueue
        ) {
            using Activity? activity = _source.StartActivity( "DetermineMultiPartUploadSuccessStatus" )?.Start( );

            bool success = true;
            List<bool> statusList = new( );
            foreach (Task<bool> task in tasks) {
                if (task.Exception != null || task.IsCanceled || task.IsCompletedSuccessfully != true) {
                    _log?.LogError( "Task was not successful." );
                    success = false;

                    if (task.Exception != null) {
                        AggregateException ex = task.Exception;
                        string logMessage = ex.Message;
                        foreach (Exception exception in ex.InnerExceptions) {
                            Type expType = exception.GetType( );
                            string[] myKeys = new string[exception.Data.Count];
                            exception.Data.Keys.CopyTo( myKeys, 0 );

                            logMessage += $"\nExceptionType : {expType}";
                            logMessage += $"\nMessage       : {exception.Message}";
                            logMessage += $"\nData          : {exception.Data}";
                            if (myKeys.Length > 0) { logMessage += "\nData          : "; }
                            for (int i = 0; i < exception.Data.Count; i++) {
                                logMessage += $"\n{i,-5}. '{myKeys[i]}' '{exception.Data[myKeys[i]]}'";
                            }
                            logMessage += $"\nHelpLink      : {exception.HelpLink}";
                            logMessage += $"\nSource        : {exception.Source}";
                            logMessage += $"\nTargetSite    : {exception.TargetSite}";
                            logMessage += $"\nHResult       : {exception.HResult}";
                            logMessage += $"\nStackTrace    : {exception.StackTrace}";
                            logMessage += $"\nInnerException: {exception.InnerException}\n";
                        }
                        _log?.LogError( "{string}", logMessage );
                    } else {
                        _log?.LogError( "There was no exception." );
                        _log?.LogError( "task.IsCompletedSuccessfully: {string}", task.IsCompletedSuccessfully );
                        _log?.LogError( "task.IsCanceled: {string}.", task.IsCanceled );
                    }
                } else {
                    statusList.Add( task.Result );
                }
            }

            if (statusList.Contains( true ) == false || success == false || filePartQueue.IsEmpty != true) {
                throw new ApplicationException( "Multi-Part upload was unsuccessful." );
            }

            activity?.Stop( );
        }
    }
}
