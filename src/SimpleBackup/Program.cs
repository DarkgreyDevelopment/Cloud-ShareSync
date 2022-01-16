using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption;
using Cloud_ShareSync.Core.Database.Sqllite;
using Cloud_ShareSync.Core.Logging;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static TelemetryLogger? s_logger;
        private static CompleteConfig? s_config;
        private static Regex[] s_excludePatterns = Array.Empty<Regex>( );
        private static FileHash? s_fileHash;
        private static ManagedChaCha20Poly1305? s_crypto;
        private static SqlliteContext? s_sqlliteContext;
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
                Inititalize( args );
                using Activity? activity = s_source.StartActivity( "Main" )?.Start( );
                PopulateFileList( );
                ValidateExistingUploads( );
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
