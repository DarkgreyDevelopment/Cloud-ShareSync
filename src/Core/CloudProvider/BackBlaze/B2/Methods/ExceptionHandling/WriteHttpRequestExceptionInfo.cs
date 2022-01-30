using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private void WriteHttpRequestExceptionInfo(
            HttpRequestException webExcp,
            int errCount,
            int thread
        ) {
            string logMessage = (errCount < _applicationData.MaxErrors) ?
                $"Thread#{thread} An error has occurred while uploading large file parts. " +
                $"This is error number {errCount} for this request." :
                $"Thread#{thread} Failed to upload large file part.";
            if (ThreadManager.FailureDetails[thread].StatusCode != null) {
                logMessage += $"\nStatus Code: {ThreadManager.FailureDetails[thread].StatusCode}";
            }

            string expMsg = webExcp.Message;
            string innerExp = webExcp.InnerException?.ToString( ) ?? "";

            switch (true) {
                case true when _regexPatterns[0].Match( expMsg ).Success:
                    logMessage += "\n" + innerExp.Split( "\n" )[0][38..];
                    break;
                case true when _regexPatterns[1].Match( expMsg ).Success:
                    logMessage += $"\n {expMsg} ";
                    logMessage += webExcp.InnerException?.Message ?? "";
                    break;
                default:
                    string[] myKeys = new string[webExcp.Data.Count];
                    webExcp.Data.Keys.CopyTo( myKeys, 0 );

                    logMessage += $"\nMessage       : {webExcp.Message}";
                    logMessage += $"\nStatusCode    : {webExcp.StatusCode}";
                    logMessage += $"\nData          : {webExcp.Data}";
                    if (myKeys.Length > 0) { logMessage += "\nData          : "; }
                    for (int i = 0; i < webExcp.Data.Count; i++) {
                        logMessage += $"\n{i,-5}. '{myKeys[i]}' '{webExcp.Data[myKeys[i]]}'";
                    }
                    logMessage += $"\nHelpLink      : {webExcp.HelpLink}";
                    logMessage += $"\nSource        : {webExcp.Source}";
                    logMessage += $"\nTargetSite    : {webExcp.TargetSite}";
                    logMessage += $"\nHResult       : {webExcp.HResult}";
                    logMessage += $"\nStackTrace    : {webExcp.StackTrace}";
                    logMessage += $"\nInnerException: {webExcp.InnerException}";
                    break;
            }

            if (errCount < _applicationData.MaxErrors) {
                _log?.LogInformation( "{string}", logMessage );
            } else {
                _log?.LogError( "{string}", logMessage );
            }
        }

    }
}
