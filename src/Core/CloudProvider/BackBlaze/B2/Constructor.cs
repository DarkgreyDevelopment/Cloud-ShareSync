using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.Cryptography;
using Microsoft.Extensions.Logging;
using Cloud_ShareSync.Core.SharedServices;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        internal B2(
            string applicationKeyId,
            string applicationKey,
            int maxErrors,
            int uploadThreads,
            string bucketName,
            string bucketId,
            ILogger? logger = null
        ) {
            _log = logger;
            _services = new CloudShareSyncServices( uploadThreads, logger );
            _fileHash = new Hashing( _log );
            _authorizationData = new( );
            _applicationData = new(
                applicationKeyId,
                applicationKey,
                maxErrors,
                uploadThreads,
                bucketName,
                bucketId
            );

            // Get Auth Client / Get initial auth data.
            _authorizationData = GetBackBlazeGeneralClient( )
                                .NewAuthReturn( _applicationData.Credentials )
                                .Result
                                .AuthData;
            if (B2ThreadManager.ThreadStats.Length == 0) {
                B2ThreadManager.Inititalize( _log, uploadThreads );
            } else if (B2ThreadManager.MaximumThreadCount < uploadThreads) {
                B2ThreadManager.UpdateMaxThreadCount( uploadThreads );
            }

            _regexPatterns = new( );
            _regexPatterns.Add(
                new( "A connection attempt failed because the connected party did not properly respond", RegexOptions.Compiled )
            );
            _regexPatterns.Add( new( "^Error while copying content to a stream.$", RegexOptions.Compiled ) );
        }

    }
}
