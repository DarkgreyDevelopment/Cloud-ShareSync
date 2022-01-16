using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using log4net;

namespace Cloud_ShareSync.Core.Cryptography {

    public class FileHash {

        private readonly ActivitySource _source = new( "FileHash" );

        private readonly ILog? _log;

        public FileHash( ILog? log = null ) { _log = log; }

        private void VerifyFileExists( FileInfo path ) => VerifyFileExists( path.FullName );

        private void VerifyFileExists( string path ) {
            using Activity? activity = _source.StartActivity( "VerifyFileExists" )?.Start( );
            if (File.Exists( path ) == false) {
                string expMessage = $"File \"{path}\" doesn't exist. Cannot get the hash of a non-existent file.";
                _log?.Fatal( expMessage );
                activity?.Stop( );
                throw new InvalidOperationException( expMessage );
            }
            activity?.Stop( );
        }

        private static FileStream GetFileStream( string path, bool sequential = true ) {
            return new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                10240,
                sequential ? FileOptions.SequentialScan : FileOptions.Asynchronous
            );
        }

        // Write bytearray out as a hexadecimal string.
        private string ConvertBytesToHexString( byte[] hashBytes ) {
            using Activity? activity = _source.StartActivity( "ConvertBytesToHexString" )?.Start( );

            StringBuilder sb = new( );

            foreach (byte b in hashBytes) { sb.Append( b.ToString( "x2" ) ); }

            activity?.Stop( );
            return sb.ToString( );
        }

        #region SHA1Hash

        public string GetSha1FileHash( string filePath ) => GetSha1FileHash( new FileInfo( filePath ) ).Result;

        internal async Task<string> GetSha1FileHash( FileInfo file ) {
            using Activity? activity = _source.StartActivity( "GetSHA1FileHash" )?.Start( );

            VerifyFileExists( file );

            // Open filestream to read file.
            using FileStream inputFileStream = GetFileStream( file.FullName );

            // Compute hash
            byte[] hashBytes = await SHA1.Create( ).ComputeHashAsync( inputFileStream );

            activity?.Stop( );
            return ConvertBytesToHexString( hashBytes );
        }

        public string GetSHA1HashForFileChunk(
            string filePath,
            byte[] data,
            long offset
        ) => GetSHA1HashForFileChunkAsync( new FileInfo( filePath ), data, offset ).Result;

        internal async Task<string> GetSHA1HashForFileChunkAsync(
            FileInfo file,
            byte[] data,
            long offset
        ) {
            using Activity? activity = _source.StartActivity( "GetSHA1HashForFileChunkAsync" )?.Start( );

            VerifyFileExists( file );

            // Open filestream to read file.
            using FileStream inputFileStream = GetFileStream( file.FullName, false );
            inputFileStream.Seek( offset, SeekOrigin.Begin );
            await inputFileStream.ReadAsync( data.AsMemory( 0, data.Length ) );

            // Compute Hash
            byte[] hashBytes = SHA1.Create( ).ComputeHash( data, 0, data.Length );

            activity?.Stop( );
            return ConvertBytesToHexString( hashBytes );
        }

        #endregion SHA1Hash

        #region SHA512Hash

        public string GetSha512StringHash( string inputString ) {
            using Activity? activity = _source.StartActivity( "GetSha512StringHash" )?.Start( );

            string results = ConvertBytesToHexString(
                SHA512.Create( )
                .ComputeHash(
                    Encoding.UTF8.GetBytes( inputString )
                )
            );

            activity?.Stop( );
            return results;
        }

        public string GetSha512FileHash( string filePath ) => GetSha512FileHash( new FileInfo( filePath ) ).Result;

        public async Task<string> GetSha512FileHash( FileInfo file ) {
            using Activity? activity = _source.StartActivity( "GetSha512FileHash" )?.Start( );

            VerifyFileExists( file );

            using FileStream inputFileStream = GetFileStream( file.FullName );

            // Create Base64 Transform Stream.
            using CryptoStream base64Stream = new( inputFileStream, new ToBase64Transform( ), CryptoStreamMode.Read );

            // Compute Hash
            byte[] hashBytes = await SHA512.Create( ).ComputeHashAsync( base64Stream );

            activity?.Stop( );
            return BitConverter.ToString( hashBytes ).Replace( "-", "" );
        }

        #endregion SHA512Hash
    }
}
