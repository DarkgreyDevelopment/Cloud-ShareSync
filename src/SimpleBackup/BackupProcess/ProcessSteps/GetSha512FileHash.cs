using System.Diagnostics;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static async Task<string> GetSha512FileHash( FileInfo path ) {
            using Activity? activity = s_source.StartActivity( "GetSha512FileHash" )?.Start( );

            if (s_fileHash == null) {
                s_logger?.ILog?.Info( "Initializing filehash." );
                s_fileHash = new( s_logger?.ILog );
            }

            s_logger?.ILog?.Info( $"Retrieving Sha512 file hash of '{path.FullName}'." );
            string sha512filehash = await s_fileHash.GetSha512FileHash( path );
            if (s_config?.BucketSync?.ObfuscateUploadedFileNames == true) {
                sha512filehash = s_fileHash.GetSha512StringHash( sha512filehash );
            }

            activity?.Stop( );
            return sha512filehash;
        }

    }
}
