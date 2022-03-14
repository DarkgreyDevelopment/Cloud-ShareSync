using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Cryptography {

    internal class Hashing {

        private readonly ActivitySource _source = new( "Hashing" );

        private readonly ILogger? _log;

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
                string expMessage = $"File \"{path.FullName}\" doesn't exist. Cannot get the hash of a non-existent file.";
                _log?.LogCritical( "{string}", expMessage );
                activity?.Stop( );
                throw new FileNotFoundException( expMessage );
            } else {
                _log?.LogDebug( "File '{string}' exists.", path.FullName );
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
        internal async Task<string> GetSha1Hash( FileInfo file ) {
            using Activity? activity = _source.StartActivity( "GetSHA1FileHash" )?.Start( );
            _log?.LogInformation( "Retrieving Sha1 hash for file '{string}'.", file.FullName );

            VerifyFileExists( file );

            // Open filestream to read file.
            using FileStream inputFileStream = GetFileStream( file.FullName );

            // Compute hash
            SystemMemoryChecker.Update( );
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms - SHA1 required by BackBlaze.
            byte[] hashBytes = await SHA1.Create( ).ComputeHashAsync( inputFileStream );
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms - SHA1 required by BackBlaze.

            // Convert to hexidecimal string
            string result = ConvertBytesToHexString( hashBytes );

            _log?.LogInformation( "Retrieved Sha1 hash for file '{string}'. Hash: {string}", file.FullName, result );
            activity?.Stop( );
            return result;
        }

        /// <summary>
        /// Returns the sha1 hash, as a hexidecimal string, for the section of <paramref name="file"/> 
        /// beginning at <paramref name="offset"/> and continuing until <paramref name="data"/>.Length
        /// </summary>
        /// <param name="file"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        internal async Task<string> GetSha1Hash(
            FileInfo file,
            byte[] data,
            long offset
        ) {
            using Activity? activity = _source.StartActivity( "GetSHA1HashForFileChunkAsync" )?.Start( );
            long end = offset + data.Length;

            _log?.LogInformation(
                "Retrieving Sha1 hash for file '{string}' section {long}-{long}.",
                file.FullName, offset, end
            );

            VerifyFileExists( file );

            // Open filestream to read file.
            using FileStream inputFileStream = GetFileStream( file.FullName, false );
            inputFileStream.Seek( offset, SeekOrigin.Begin );
            await inputFileStream.ReadAsync( data.AsMemory( 0, data.Length ) );

            // Compute Hash
            SystemMemoryChecker.Update( );

#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms - SHA1 required by BackBlaze.
            byte[] hashBytes = SHA1.Create( ).ComputeHash( data, 0, data.Length );
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms - SHA1 required by BackBlaze.

            // Convert to hexidecimal string
            string result = ConvertBytesToHexString( hashBytes );

            _log?.LogInformation(
                "Retrieved Sha1 hash for file '{string}' section {long}-{long}. Hash: {string}",
                file.FullName, offset, end, result
            );
            activity?.Stop( );
            return result;
        }

        #endregion SHA1Hash


        #region SHA512Hash

        /// <summary>
        /// Returns the sha512 hash, as a hexidecimal string, of a given <paramref name="inputString"/>.
        /// </summary>
        /// <param name="inputString"></param>
        public string GetSha512Hash( string inputString ) {
            using Activity? activity = _source.StartActivity( "GetSha512StringHash" )?.Start( );
            _log?.LogInformation( "Retrieving Sha512 hash for inputstring '{string}'.", inputString );

            SystemMemoryChecker.Update( );
            string result = ConvertBytesToHexString(
                SHA512.Create( ).ComputeHash( Encoding.UTF8.GetBytes( inputString ) )
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
            SystemMemoryChecker.Update( );
            byte[] hashBytes = await SHA512.Create( ).ComputeHashAsync( base64Stream );

            // Convert to hexidecimal string
            string result = ConvertBytesToHexString( hashBytes );

            _log?.LogInformation( "Retrieved Sha512 hash for file '{string}'. Hash: {string}", file.FullName, result );
            activity?.Stop( );
            return result;
        }

        #endregion SHA512Hash

    }
}
