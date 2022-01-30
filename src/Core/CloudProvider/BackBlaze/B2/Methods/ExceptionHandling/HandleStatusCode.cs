using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private void HandleStatusCode(
            HttpRequestException webExcp,
            int? statusCode
        ) {
            switch (statusCode) {
                case 403:
                    _log?.LogCritical( "{string}", webExcp.Message );
                    throw new Exception( "Received StatusCode 403.", webExcp );
                default:
                    break;
            }
        }

    }
}
