using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.SimpleBackup.Types;
using Cloud_ShareSync.SimpleBackup.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleBackup.Workers {

    public class UploadFileWorker : BackgroundService {
        private static readonly ActivitySource s_source = new( "Cloud_ShareSync.SimpleBackup.UploadFileWorker" );

        public static readonly ConcurrentQueue<UploadFileInput> Queue = new( );

        private readonly ILogger<UploadFileWorker> _logger;
        private readonly IUploadFileProcess _uploadFileProcess;

        public UploadFileWorker(
            IUploadFileProcess uploadFileProcess,
            ILogger<UploadFileWorker> logger
        ) {
            _uploadFileProcess = uploadFileProcess;
            _logger = logger;
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
            using Activity? activity = s_source.StartActivity( "ExecuteAsync" )?.Start( );
            _logger.LogInformation( "Starting Upload File Process." );

            while (stoppingToken.IsCancellationRequested == false) {
                if (Queue.IsEmpty) { Thread.Sleep( 10000 ); }
                bool deQueue = Queue.TryDequeue( out UploadFileInput? ufInput );
                if (deQueue && ufInput != null) {
                    await _uploadFileProcess.Process( ufInput );
                }
            }

            _logger.LogInformation( "Exiting Upload File Process." );
            activity?.Stop( );
        }
    }
}

