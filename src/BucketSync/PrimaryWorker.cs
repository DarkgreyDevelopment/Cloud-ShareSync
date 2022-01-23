using Cloud_ShareSync.BucketSync.Process;
/*
namespace Cloud_ShareSync.BucketSync {
    public class PrimaryWorker : BackgroundService {
        private readonly ILogger<PrimaryWorker> _logger;
        private readonly ILocalSyncProcess _localSyncProcess;

        public PrimaryWorker(
            ILocalSyncProcess localSyncProcess,
            ILogger<PrimaryWorker> logger
        ) {
            _localSyncProcess = localSyncProcess;
            _logger = logger;
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {

            while (!stoppingToken.IsCancellationRequested) {
                _logger.LogInformation( "Worker running at: {time}", DateTimeOffset.Now );
                _localSyncProcess.Startup( );
                await _localSyncProcess.Process( );
                Thread.Sleep( 5000 );
            }
        }
    }
}
*/
