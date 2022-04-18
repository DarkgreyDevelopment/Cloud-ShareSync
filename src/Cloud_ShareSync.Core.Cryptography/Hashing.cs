using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Cryptography {

    public class Hashing {

        private readonly ActivitySource _source = new( "Hashing" );

        private readonly ILogger? _log;

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms - SHA1 required by BackBlaze.
        private readonly SHA1 _sha1 = SHA1.Create( );
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms - SHA1 required by BackBlaze.
        private readonly SHA512 _sha512 = SHA512.Create( );

        public Hashing( ILogger? log = null ) { _log = log; }

        #region Shared Private Methods

        /// <summary>
        /// Check if a file exists at <paramref name="path"/> . If not throw a file not found exception.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="FileNotFoundException"></exception>
        private void VerifyFileExists( FileInfo path ) {
            using Activity? activity = _source.StartActivity( "VerifyFileExists" )?.Start( );
            if (File.Exists( path.FullName ) == false) {
                _log?.LogCritical(
                    "File '{string}' doesn't exist. Cannot get the hash of a non-existent file.",
                    path.FullName
                );
                activity?.Stop( );
                throw new FileNotFoundException( path.FullName );
            }
            activity?.Stop( );
        }

        /// <summary>
        /// Common method for retrieving a filestream w/ either SequentialScan or Asyncronous FileOptions
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sequential"></param>
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

        /// <summary>
        /// Converts bytearray into hexidecimal string
        /// </summary>
        /// <param name="hashBytes"></param>
        private string ConvertBytesToHexString( byte[] hashBytes ) {
            using Activity? activity = _source.StartActivity( "ConvertBytesToHexString" )?.Start( );

            StringBuilder sb = new( );

            foreach (byte b in hashBytes) { _ = sb.Append( b.ToString( "x2" ) ); }

            activity?.Stop( );
            return sb.ToString( );
        }

        #endregion Shared Private Methods


        #region SHA1Hash

        /// <summary>
        /// Returns the sha1 hash of the <paramref name="file"/> as a hexidecimal string.
        /// </summary>
        /// <param name="file"></param>
        public async Task<string> GetSha1Hash( FileInfo file ) {
            using Activity? activity = _source.StartActivity( "GetSHA1FileHash" )?.Start( );
            _log?.LogInformation( "Retrieving Sha1 hash for file '{string}'.", file.FullName );

            VerifyFileExists( file );

            // Open filestream to read file.
            using FileStream inputFileStream = GetFileStream( file.FullName );

            // Compute hash
            byte[] hashBytes = await _sha1.ComputeHashAsync( inputFileStream );

            activity?.Stop( );
            return ConvertBytesToHexString( hashBytes ); // Convert to hexidecimal string
        }

        /// <summary>
        /// Returns the sha1 hash, as a hexidecimal string, for the section of <paramref name="file"/> 
        /// beginning at <paramref name="offset"/> and continuing until <paramref name="data"/>.Length
        /// </summary>
        /// <param name="file"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public async Task<string> GetSha1Hash(
            FileInfo file,
            byte[] data,
            long offset
        ) {
            using Activity? activity = _source.StartActivity( "GetSHA1HashForFileChunkAsync" )?.Start( );
            _log?.LogInformation(
                "Retrieving Sha1 hash for file '{string}' section {long}-{long}.",
                file.FullName, offset, offset + data.Length
            );

            VerifyFileExists( file );

            // Open filestream to read file.
            using FileStream inputFileStream = GetFileStream( file.FullName, false );
            _ = inputFileStream.Seek( offset, SeekOrigin.Begin );
            _ = await inputFileStream.ReadAsync( data.AsMemory( 0, data.Length ) );

            // Compute Hash
            byte[] hashBytes = _sha1.ComputeHash( data, 0, data.Length );

            activity?.Stop( );
            return ConvertBytesToHexString( hashBytes ); // Convert to hexidecimal string
        }

        #endregion SHA1Hash


        #region SHA512Hash

        /// <summary>
        /// Returns the sha512 hash of the <paramref name="inputString"/> as a hexidecimal string.
        /// </summary>
        /// <param name="inputString"></param>
        public string GetSha512Hash( string inputString ) {
            using Activity? activity = _source.StartActivity( "GetSha512StringHash" )?.Start( );
            _log?.LogInformation( "Retrieving Sha512 hash for inputstring '{string}'.", inputString );

            string result = ConvertBytesToHexString(
                _sha512.ComputeHash( Encoding.UTF8.GetBytes( inputString ) )
            );

            _log?.LogInformation( "Retrieved Sha512 hash for inputstring '{string}'. Hash: {string}", inputString, result );
            activity?.Stop( );
            return result;
        }

        /// <summary>
        /// Returns the sha512 hash of the <paramref name="file"/> as a hexidecimal string.
        /// </summary>
        /// <param name="file"></param>
        public async Task<string> GetSha512Hash( FileInfo file ) {
            using Activity? activity = _source.StartActivity( "GetSha512FileHash" )?.Start( );
            _log?.LogInformation( "Retrieving Sha512 hash for file '{string}'.", file.FullName );

            VerifyFileExists( file );

            using FileStream inputFileStream = GetFileStream( file.FullName );

            // Create Base64 Transform Stream.
            using CryptoStream base64Stream = new( inputFileStream, new ToBase64Transform( ), CryptoStreamMode.Read );

            // Compute Hash
            byte[] hashBytes = await _sha512.ComputeHashAsync( base64Stream );

            activity?.Stop( );
            return ConvertBytesToHexString( hashBytes ); // Convert to hexidecimal string
        }

        #endregion SHA512Hash

    }
}
