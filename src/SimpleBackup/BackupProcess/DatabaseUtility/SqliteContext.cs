using Cloud_ShareSync.Core.Database.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static SqliteContext GetSqliteContext( ) {
            if (s_services == null) { throw new InvalidOperationException( "Cannot get context if db service is not initialized." ); }
            s_semaphore.Wait( );
            SqliteContext result = s_services.Services.GetRequiredService<SqliteContext>( );
            return result;
        }

        private static void ReleaseSqliteContext( ) { s_semaphore.Release( ); }

    }
}
