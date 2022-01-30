using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.Cryptography;
using Microsoft.Extensions.Logging;
using Cloud_ShareSync.Core.SharedServices;

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
            _fileHash = new FileHash( _log );
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
            ThreadManager = new( _log, uploadThreads );

            _regexPatterns = new( );
            _regexPatterns.Add(
                new( "A connection attempt failed because the connected party did not properly respond", RegexOptions.Compiled )
            );
            _regexPatterns.Add( new( "^Error while copying content to a stream.$", RegexOptions.Compiled ) );
        }

    }
}
