using System.Diagnostics;
using Cloud_ShareSync.Core.Database.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static SqliteContext GetSqliteContext( ) {
            using Activity? activity = s_source.StartActivity( "GetSqliteContext" )?.Start( );
            if (s_services == null) { throw new InvalidOperationException( "Cannot get context if db service is not initialized." ); }
            s_logger?.ILog?.Debug( "Waiting for SqliteContext Semaphore" );
            s_semaphore.Wait( );
            SqliteContext result = s_services.Services.GetRequiredService<SqliteContext>( );
            activity?.Stop( );
            return result;
        }

        private static void ReleaseSqliteContext( ) {
            using Activity? activity = s_source.StartActivity( "ReleaseSqliteContext" )?.Start( );
            s_logger?.ILog?.Debug( "Releasing SqliteContext Semaphore" );
            s_semaphore.Release( );
            activity?.Stop( );
        }

    }
}
