using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.Core.SharedServices;
using Cloud_ShareSync.SimpleBackup.BackupService;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static TelemetryLogger? s_logger;
        private static CompleteConfig? s_config;
        private static Regex[] s_excludePatterns = Array.Empty<Regex>( );
        private static FileHash? s_fileHash;
        private static BackBlazeB2? s_backBlaze;
        private static CloudShareSyncServices? s_services;
        private static UploadFileProcess? s_uploadProcess;
        private static readonly SemaphoreSlim s_semaphore = new( 0, 1 );
        private static readonly ConcurrentQueue<string> s_fileUploadQueue = new( );
        private static readonly ActivitySource s_source = new( "Cloud_ShareSync.SimpleBackup.Program" );
        private static readonly string[] s_sourceList = new string[] {
            "Cloud_ShareSync.SimpleBackup.Program",
            "B2",
            "BackBlazeB2.PublicInterface",
            "CompressionInterface",
            "Config",
            "FileHash",
            "LocalSyncProcess",
            "ManagedChaCha20Poly1305",
            "MimeType",
            "UniquePassword"
        };

        public static async Task Main( string[] args ) {
            try {
                Initialize( args );
                using Activity? activity = s_source.StartActivity( "Main" )?.Start( );
                if (s_config?.SimpleBackup == null) {
                    throw new InvalidOperationException( "Cannot continue if SimpleBackup Config is null." );
                }
                PopulateFileList( s_fileUploadQueue, s_excludePatterns, s_config.SimpleBackup, s_logger?.ILog );
                await BackupProcess( );
                s_logger?.ILog?.Info( "Simple backup completed." );
                activity?.Stop( );
            } catch (Exception e) {
                if (s_logger?.ILog == null) {
                    Console.WriteLine( e.ToString( ) );
                } else {
                    s_logger?.ILog.Fatal( e.ToString( ) );
                }
            }
        }


    }
}
