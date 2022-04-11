using System.Collections;
using System.Text;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Threading;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private void LogHttpRequestExceptionInfo(
            HttpRequestException webExcp,
            int errCount,
            int thread
        ) {

            StringBuilder msg = new(
                (errCount < _applicationData.MaxErrors) ?
                    $"Thread#{thread} An error has occurred while uploading large file parts. " +
                        $"This is error number {errCount} for this request." :
                    $"Thread#{thread} Failed to upload large file part."
            );

            if (B2ThreadManager.FailureDetails[thread].StatusCode != null) {
                _ = msg.Append( $"\nStatus Code: {B2ThreadManager.FailureDetails[thread].StatusCode}" );
            }

            switch (true) {
                case true when MatchRegexPattern0( webExcp.Message ):
                    _ = msg.Append( $"\n{webExcp.InnerException?.ToString( ).Split( "\n" )[0][38..]}" );
                    break;
                case true when MatchRegexPattern1( webExcp.Message ):
                    _ = msg.Append( $"\n {webExcp.Message} {webExcp.InnerException?.Message}" );
                    break;
                default:
                    AddExceptionDataToStringBuilder( msg, webExcp );
                    break;
            }

            if (errCount < _applicationData.MaxErrors) {
                _log?.LogInformation( "{string}", msg.ToString( ) );
            } else {
                _log?.LogError( "{string}", msg.ToString( ) );
            }
        }

        private bool MatchRegexPattern0( string msg ) =>
            _regexPatterns[0].Match( msg ).Success;

        private bool MatchRegexPattern1( string msg ) =>
            _regexPatterns[1].Match( msg ).Success;

        private static void AddExceptionDataToStringBuilder( StringBuilder msg, HttpRequestException webExcp ) {
            _ = msg.Append( $"\nMessage       : {webExcp.Message}" );
            _ = msg.Append( $"\nStatusCode    : {webExcp.StatusCode}" );
            AddDataPropertyToStringBuilder( msg, webExcp );
            _ = msg.Append( $"\nHelpLink      : {webExcp.HelpLink}" );
            _ = msg.Append( $"\nSource        : {webExcp.Source}" );
            _ = msg.Append( $"\nTargetSite    : {webExcp.TargetSite}" );
            _ = msg.Append( $"\nHResult       : {webExcp.HResult}" );
            _ = msg.Append( $"\nStackTrace    : {webExcp.StackTrace}" );
            _ = msg.Append( $"\nInnerException: {webExcp.InnerException}" );
        }

        private static void AddDataPropertyToStringBuilder( StringBuilder msg, HttpRequestException webExcp ) {
            if (webExcp.Data.Count > 0) {
                _ = msg.Append( "\nData          : " );
                foreach (DictionaryEntry de in webExcp.Data) {
                    _ = msg.Append( $"\n              : '{de.Key,-20}' Value: '{de.Value}'" );
                }
            }
        }

    }
}
