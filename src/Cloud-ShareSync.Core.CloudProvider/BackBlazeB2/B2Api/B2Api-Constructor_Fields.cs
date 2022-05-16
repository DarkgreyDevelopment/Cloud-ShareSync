using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Cloud_ShareSync.Core.CloudProvider.SharedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2 {
    public partial class B2Api {

        public B2Api(
            string applicationKeyId,
            string applicationKey,
            string bucketName,
            string bucketId,
            int maxErrors,
            int httpThreads,
            ILogger? logger
        ) {
            _log = logger;
            _initData = new(
                applicationKeyId,
                applicationKey,
                bucketName,
                bucketId,
                maxErrors > 0 ? maxErrors : 1,
                httpThreads > 0 ? httpThreads : 1
            );
            _services = new HttpClientServices( httpThreads, _log );
            AuthToken = GetAuthData( ).GetAwaiter( ).GetResult( );
            LargeFileSize = DeriveLargeFileSize( );
        }

        private readonly ActivitySource _source = new( "B2Api" );
        private readonly ILogger? _log;
        private readonly InitializationData _initData;
        private readonly HttpClientServices _services;
        private int LargeFileSize { get; set; }
        private AuthorizeAccount AuthToken { get; set; }

        private async Task UpdateAuthData( ) {
            AuthToken = await GetAuthData( );
            LargeFileSize = DeriveLargeFileSize( );
        }

        private int DeriveLargeFileSize( ) {
            long derivedLargeFileSize = (AuthToken.absoluteMinimumPartSize * 2) * _initData.HttpThreads;
            int recommendedLargeFileSize = AuthToken.recommendedPartSize * 2;

            return derivedLargeFileSize < recommendedLargeFileSize
                ? (int)derivedLargeFileSize
                : recommendedLargeFileSize;
        }

        private HttpClient GetHttpClient( ) =>
            _services.Services.GetRequiredService<CloudShareSyncHttpClient>( ).HttpClient;

        private async Task<AuthorizeAccount> GetAuthData( ) => await AuthorizeAccount
            .CallApi(
                _initData.Credentials,
                GetHttpClient( ),
                _initData.MaxErrors,
                _log
            );
    }
}
