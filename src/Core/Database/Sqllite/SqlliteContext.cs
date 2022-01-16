using Microsoft.EntityFrameworkCore;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.Core.Database.Sqllite {
#nullable disable
    public class SqlliteContext : DbContext {
        public DbSet<PrimaryTable> CoreData { get; set; }
        public DbSet<EncryptionTable> EncryptionData { get; set; }
        public DbSet<CompressionTable> CompressionData { get; set; }
        public DbSet<BackBlazeB2Table> BackBlazeB2Data { get; set; }

        public string DbPath { get; }

        public SqlliteContext( string path ) {
            DbPath = Directory.Exists( path ) ? Path.Join( path, "CloudShareSync.db" ) : path;

            if (File.Exists( DbPath ) == false) {
                Database.EnsureDeleted( );
                Database.EnsureCreated( );
            }
        }

        protected override void OnModelCreating( ModelBuilder modelBuilder ) {

            modelBuilder.Entity<PrimaryTable>( )
                .HasKey( x => x.Id );
            modelBuilder.Entity<PrimaryTable>( )
                .Property( o => o.Id )
                .IsRequired( );
        }

        protected override void OnConfiguring( DbContextOptionsBuilder options )
            => options.UseSqlite( $"Data Source={DbPath}" );
    }
#nullable enable
}
