using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption;
using Cloud_ShareSync.Core.Database.Sqlite;
using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.DependencyInjection;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static TelemetryLogger? s_logger;
        private static CompleteConfig? s_config;
        private static Regex[] s_excludePatterns = Array.Empty<Regex>( );
        private static FileHash? s_fileHash;
        private static ManagedChaCha20Poly1305? s_crypto;
        private static BackBlazeB2? s_backBlaze;
        private static CloudShareSyncServices? s_services;
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
                s_semaphore.Release( 1 );
                Initialize( args );
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

        private static SqliteContext GetSqliteContext( ) {
            if (s_services == null) { throw new InvalidOperationException( "Cannot get context if db service is not initialized." ); }
            s_logger?.ILog?.Debug( "Waiting for SqliteContext Semaphore" );
            s_semaphore.Wait( );
            return s_services.Services.GetRequiredService<SqliteContext>( );
        }

        private static void ReleaseSqliteContext( ) {
            s_logger?.ILog?.Debug( "Releasing SqliteContext Semaphore" );
            s_semaphore.Release( );
        }

    }
}
