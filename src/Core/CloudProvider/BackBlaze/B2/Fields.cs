using System.Diagnostics;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private readonly ActivitySource _source = new( "B2" );
        private readonly ILogger? _log;

        // Configuration vars
        internal const string AuthorizationURI = "https://api.backblazeb2.com/b2api/v2/b2_authorize_account";
        internal const string UserAgent = "Cloud-ShareSync_BackBlazeB2/0.0.1+dotnet/6.0";

        // Set by Ctor.
        private readonly InitializationData _applicationData;
        private readonly List<Regex> _regexPatterns;
        private readonly CloudShareSyncServices _services;
        private readonly Hashing _fileHash;
        internal readonly B2ThreadManager ThreadManager;

        // Set by valid authorization process
        private AuthProcessData _authorizationData;
        internal int? RecommendedPartSize => _authorizationData.RecommendedPartSize;
        internal int? AbsoluteMinimumPartSize => _authorizationData.AbsoluteMinimumPartSize;

    }
}
