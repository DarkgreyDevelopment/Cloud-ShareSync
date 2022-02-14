using System.Diagnostics;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Database.Sqlite;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void ConfigureDatabase( DatabaseConfig config ) {
            using Activity? activity = s_source.StartActivity( "Initialize.ConfigureDatabase" )?.Start( );

            CloudShareSyncServices services = new( config.SqliteDBPath, s_logger );

            SqliteContext sqliteContext = services.Services.GetRequiredService<SqliteContext>( );

            int coreTableCount = (from obj in sqliteContext.CoreData where obj.Id >= 0 select obj).Count( );
            int encryptedCount = (from obj in sqliteContext.EncryptionData where obj.Id >= 0 select obj).Count( );
            int compressdCount = (from obj in sqliteContext.CompressionData where obj.Id >= 0 select obj).Count( );
            int backBlazeCount = (from obj in sqliteContext.BackBlazeB2Data where obj.Id >= 0 select obj).Count( );
            s_logger?.LogInformation( "Database Initialized." );
            s_logger?.LogInformation( "Core Table      : {string}", coreTableCount );
            s_logger?.LogInformation( "Encrypted Table : {string}", encryptedCount );
            s_logger?.LogInformation( "Compressed Table: {string}", compressdCount );
            s_logger?.LogInformation( "BackBlaze Table : {string}", backBlazeCount );

            activity?.Stop( );
        }
    }
}
