using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption;
using Cloud_ShareSync.Core.Database.Sqlite;
using Cloud_ShareSync.Core.Logging;

namespace Cloud_ShareSync.SimpleRestore {

    public partial class Program {

        private static TelemetryLogger? s_logger;
        private static CompleteConfig? s_config;
        private static FileHash? s_fileHash;
        private static ManagedChaCha20Poly1305? s_crypto;
        private static BackBlazeB2? s_backBlaze;
        private static SqliteContext? s_sqliteContext;
        private static readonly ActivitySource s_source = new( "Cloud_ShareSync.SimpleRestore.Program" );
        private static readonly string[] s_sourceList = new string[] {
            "Cloud_ShareSync.SimpleRestore.Program",
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

                s_logger?.ILog?.Info( "Simple restore completed." );
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
