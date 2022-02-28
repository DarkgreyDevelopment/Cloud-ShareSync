using Cloud_ShareSync.Core.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cloud_ShareSync.Core.Database {
#nullable disable
    internal class SqliteContext : DbContext {

        public static string DatabasePath { get; set; }

        public DbSet<PrimaryTable> CoreData { get; set; }
        public DbSet<EncryptionTable> EncryptionData { get; set; }
        public DbSet<CompressionTable> CompressionData { get; set; }
        public DbSet<BackBlazeB2Table> BackBlazeB2Data { get; set; }

        public string DbPath { get; private set; }

        public SqliteContext( ) : this( new( DatabasePath ) ) { }

        public SqliteContext( FileInfo path ) {
            DbPath = path.FullName;

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

        protected override void OnConfiguring( DbContextOptionsBuilder options ) =>
            options.UseSqlite( $"Data Source={DbPath}" );

        public static string DetermineDbPath( string path ) {
            string result = "CloudShareSync.db";

            if (Directory.Exists( path )) {
                result = Path.Join( path, result );
            } else if (File.Exists( path )) {
                result = path;
            } else if (path != null) {
                string[] pathPieces = path.Split(
                    new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries
                );
                if (path.StartsWith( '/' )) {
                    List<string> pieces = new( ) {
                        "/"
                    };
                    pieces.AddRange( pathPieces );
                    pathPieces = pieces.ToArray( );
                } else if (path.StartsWith( "\\\\" )) {
                    List<string> pieces = new( ) {
                        "\\\\"
                    };
                    pieces.AddRange( pathPieces );
                    pathPieces = pieces.ToArray( );
                }
                string pathPieceCombo = Path.Join( pathPieces.SkipLast( 1 ).ToArray( ) );
                if (Directory.Exists( pathPieceCombo )) {
                    result = path;
                } else {
                }
            }

            return result;
        }
    }
#nullable enable
}
