using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.SimpleBackup.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleBackup.Workers {

    public class PrepUploadFileWorker : BackgroundService {
        private static readonly ActivitySource s_source = new( "Cloud_ShareSync.SimpleBackup.PrepUploadFileWorker" );

        public static readonly ConcurrentQueue<string> Queue = new( );

        private readonly ILogger<PrepUploadFileWorker> _logger;
        private readonly IPrepUploadFileProcess _prepUploadFileProcess;

        public PrepUploadFileWorker(
            IPrepUploadFileProcess prepUploadFileProcess,
            ILogger<PrepUploadFileWorker> logger
        ) {
            _prepUploadFileProcess = prepUploadFileProcess;
            _logger = logger;
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
            using Activity? activity = s_source.StartActivity( "ExecuteAsync" )?.Start( );
            _logger.LogInformation( "Starting Prep Upload File Process." );

            do {
                await _prepUploadFileProcess.Prep( Queue );
            } while (stoppingToken.IsCancellationRequested == false && Queue.IsEmpty == false);

            _logger.LogInformation( "Exiting Prep Upload File Process." );
            activity?.Stop( );
        }
    }
}

