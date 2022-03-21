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
                _ = Database.EnsureDeleted( );
                _ = Database.EnsureCreated( );
            }
        }

        protected override void OnModelCreating( ModelBuilder modelBuilder ) {
            _ = modelBuilder.Entity<PrimaryTable>( )
                .HasKey( x => x.Id );
            _ = modelBuilder.Entity<PrimaryTable>( )
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
            } else {
                if (Directory.Exists( ParseDBDirectoryPath( path ) )) { result = path; }
            }

            return result;
        }

        // Assumes that we've received the path to file we should create.
        internal static string ParseDBDirectoryPath( string path ) {
            string[] pathPieces = path.Split(
                                new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                                StringSplitOptions.RemoveEmptyEntries
                            );

            if (path.StartsWith( '/' )) { // Re-Add Linux root
                List<string> pieces = new( ) { "/" };
                pieces.AddRange( pathPieces );
                pathPieces = pieces.ToArray( );
            } else if (path.StartsWith( "\\\\" )) { // Re-Add UNC root
                List<string> pieces = new( ) {
                    "\\\\"
                };
                pieces.AddRange( pathPieces );
                pathPieces = pieces.ToArray( );
            }

            return Path.Join( pathPieces.SkipLast( 1 ).ToArray( ) );
        }
    }
#nullable enable
}
