using System.Diagnostics;
using Cloud_ShareSync.Core.Configuration.Types.Features;
using Cloud_ShareSync.Core.Database.Sqlite;
using Cloud_ShareSync.Core.SharedServices;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void ConfigureDatabase( DatabaseConfig config ) {
            using Activity? activity = s_source.StartActivity( "Initialize.ConfigureDatabase" )?.Start( );

            s_services = new CloudShareSyncServices( config.SqliteDBPath, s_logger );

            s_semaphore.Release( 1 ); // Ensure we can enter the semaphore.
            SqliteContext sqliteContext = GetSqliteContext( );
            int coreTableCount = (from obj in sqliteContext.CoreData where obj.Id >= 0 select obj).Count( );
            int encryptedCount = (from obj in sqliteContext.EncryptionData where obj.Id >= 0 select obj).Count( );
            int compressdCount = (from obj in sqliteContext.CompressionData where obj.Id >= 0 select obj).Count( );
            int backBlazeCount = (from obj in sqliteContext.BackBlazeB2Data where obj.Id >= 0 select obj).Count( );
            ReleaseSqliteContext( );
            s_logger?.ILog?.Info( "Database Initialized." );
            s_logger?.ILog?.Info( $"Core Table      : {coreTableCount}" );
            s_logger?.ILog?.Info( $"Encrypted Table : {encryptedCount}" );
            s_logger?.ILog?.Info( $"Compressed Table: {compressdCount}" );
            s_logger?.ILog?.Info( $"BackBlaze Table : {backBlazeCount}" );

            activity?.Stop( );
        }

    }
}
