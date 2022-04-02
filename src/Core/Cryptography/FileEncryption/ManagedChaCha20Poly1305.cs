using System.Diagnostics;
using System.Security.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption.Types;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Cryptography.FileEncryption {
    /// <summary>
    /// Creates a managed instance of the <see cref="System.Security.Cryptography.ChaCha20Poly1305"/> class.
    /// 
    /// <see cref="ManagedChaCha20Poly1305"/> handles encrypting and decrypting files.
    /// </summary>
    public class ManagedChaCha20Poly1305 {

        /// <summary>
        /// <para>
        /// Describes ChaCha20Poly1305 platform support status.
        /// </para>
        /// See: <seealso cref="System.Security.Cryptography.ChaCha20Poly1305.IsSupported"/>
        /// </summary>
        public static bool PlatformSupported { get { return !OperatingSystem.IsIOS( ) && ChaCha20Poly1305.IsSupported; } }

        /// <summary>
        /// The number of bytes to process before changing nonces.
        /// </summary>
        public const int MaxValue = 500000000;

        private readonly ActivitySource _source = new( "ManagedChaCha20Poly1305" );
        private readonly ILogger? _log;

        public ManagedChaCha20Poly1305( ILogger? log = null ) {
            if (PlatformSupported == false) {
                throw new InvalidOperationException(
                    "ChaCha20Poly1305 isn't supported on this platform. " +
                    "Cannot create a managed instance."
                );
            }
            _log = log;
        }

        public ManagedChaCha20Poly1305( ) : this( null ) { }

        #region Encryption

        /// <summary>
        /// <para>
        /// Uses <see cref="ChaCha20Poly1305"/> initialized with a <paramref name="key"/> to 
        /// encrypt <paramref name="plaintextFile"/> into <paramref name="cypherTxtFile"/>.
        /// <see cref="ManagedChaCha20Poly1305DecryptionData"/> is returned.
        /// </para>
        /// <see cref="ManagedChaCha20Poly1305DecryptionData"/> can also optionally be written to a <paramref name="keyFile"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="plaintextFile"></param>
        /// <param name="cypherTxtFile"></param>
        /// <param name="keyFile"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<ManagedChaCha20Poly1305DecryptionData> Encrypt(
            byte[] key,
            FileInfo plaintextFile,
            FileInfo cypherTxtFile,
            FileInfo? keyFile = null
        ) {
            using Activity? activity = _source.StartActivity( "Encrypt" )?.Start( );

            EnsureFileExistsAndHasData( plaintextFile );

            WriteEncryptLogMessages( true, plaintextFile, cypherTxtFile, keyFile );

            ManagedChaCha20Poly1305DecryptionData decryptionData = await EncryptFile( key, plaintextFile, cypherTxtFile );

            await WriteDecryptionDataToKeyFile( keyFile, decryptionData );

            WriteEncryptLogMessages( false, plaintextFile, cypherTxtFile, keyFile );

            activity?.Stop( );
            return decryptionData;
        }

        internal void WriteEncryptLogMessages(
            bool message1,
            FileInfo plaintextFile,
            FileInfo cypherTxtFile,
            FileInfo? keyFile
        ) {

            if (message1) {
                string encDbgStr = $"Encrypting '{plaintextFile.FullName}' into '{cypherTxtFile.FullName}'.";
                encDbgStr += keyFile == null ? "" : $" Decryption keyfile will be located at '{keyFile.FullName}'.";
                _log?.LogInformation( "{string}", encDbgStr );
            } else {
                string enc2DbgStr = $"Encrypted '{plaintextFile.FullName}' into '{cypherTxtFile.FullName}'. ";
                enc2DbgStr += keyFile == null ? "" : $"Decryption keyfile is located at '{keyFile.FullName}'. ";
                _log?.LogInformation( "{string}", enc2DbgStr );
            }

        }

        internal async Task WriteDecryptionDataToKeyFile(
            FileInfo? keyFile,
            ManagedChaCha20Poly1305DecryptionData decryptionData
        ) {
            _log?.LogDebug( "Decryption Data: {string}", decryptionData.ToString( ) );
            if (keyFile != null) {
                await File.WriteAllTextAsync(
                    keyFile.FullName,
                    decryptionData.ToString( )
                );
            }
        }

        /// <summary>
        /// Uses <see cref="ChaCha20Poly1305"/> initialized with <paramref name="key"/> to 
        /// encrypt <paramref name="plaintextFile"/> into <paramref name="cypherTxtFile"/>.
        /// DecryptionData is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="plaintextFile"></param>
        /// <param name="cypherTxtFile"></param>
        private async Task<ManagedChaCha20Poly1305DecryptionData> EncryptFile(
            byte[] key,
            FileInfo plaintextFile,
            FileInfo cypherTxtFile
        ) {
            using Activity? activity = _source.StartActivity( "EncryptFile" )?.Start( );

            ChaCha20Poly1305 chaPoly = new( key );
            List<byte[]> uniqueNonces = GetNonces( plaintextFile.Length );
            List<ManagedChaCha20Poly1305DecryptionKeyNote> keyNoteList = new( );

            await EncryptFileChunkLoop(
                plaintextFile,
                cypherTxtFile,
                chaPoly,
                uniqueNonces,
                keyNoteList
            );

            SystemMemoryChecker.Update( );

            activity?.Stop( );
            return new ManagedChaCha20Poly1305DecryptionData( key, keyNoteList );
        }

        internal async Task EncryptFileChunkLoop(
            FileInfo plaintextFile,
            FileInfo cypherTxtFile,
            ChaCha20Poly1305 chaPoly,
            List<byte[]> uniqueNonces,
            List<ManagedChaCha20Poly1305DecryptionKeyNote> keyNoteList,
            long processedBytes = 0,
            int chunkCount = 0
        ) {
            while (processedBytes < plaintextFile.Length) {
                SystemMemoryChecker.Update( );
                byte[] plaintext = GetByteArray(
                                        chunkCount,
                                        uniqueNonces.Count,
                                        processedBytes,
                                        plaintextFile.Length
                                    );

                keyNoteList.Add(
                    await EncryptFileChunk(
                        chaPoly,
                        uniqueNonces[chunkCount],
                        plaintext,
                        plaintextFile,
                        cypherTxtFile,
                        chunkCount,
                        processedBytes
                    )
                );

                processedBytes += plaintext.Length;
                chunkCount++;
            }
        }

        internal byte[] GetByteArray(
            int chunkCount,
            int nonceCount,
            long processedBytes,
            long plaintextFileLength
        ) => ((chunkCount + 1) == nonceCount) ?
            new byte[plaintextFileLength - processedBytes] :
            new byte[MaxValue];

        /// <summary>
        /// Uses <paramref name="chaPoly"/> to read a <paramref name="plaintext"/>.Length section of 
        /// <paramref name="plaintextFile"/> from the <paramref name="offset"/> and encrypt it into 
        /// <paramref name="cypherTxtFile"/>.
        /// </summary>
        /// <returns><see cref="ManagedChaCha20Poly1305DecryptionKeyNote"/> (<paramref name="nonce"/>, tag, <paramref name="order"/>)
        /// for encrypted section.</returns>
        /// <param name="chaPoly"></param>
        /// <param name="nonce"></param>
        /// <param name="plaintext"></param>
        /// <param name="plaintextFile"></param>
        /// <param name="cypherTxtFile"></param>
        /// <param name="order"></param>
        /// <param name="offset"></param>
        private async Task<ManagedChaCha20Poly1305DecryptionKeyNote> EncryptFileChunk(
            ChaCha20Poly1305 chaPoly,
            byte[] nonce,
            byte[] plaintext,
            FileInfo plaintextFile,
            FileInfo cypherTxtFile,
            int order,
            long offset
        ) {
            using Activity? activity = _source.StartActivity( "EncryptFileChunk" )?.Start( );

            // Initialize
            byte[] tag = new byte[16];
            byte[] cypherTxt = new byte[plaintext.Length];

            // Encrypt Data Into CypherTxt
            chaPoly.Encrypt(
                nonce,
                await ReadFileChunkFromOffset( plaintextFile, plaintext, offset ),
                cypherTxt,
                tag
            );

            // Append cypherTxt to cypherTxtFile.
            await AppendFileChunk( cypherTxtFile, cypherTxt, order );

            activity?.Stop( );
            return new ManagedChaCha20Poly1305DecryptionKeyNote( nonce, tag, order );
        }

        /// <summary>
        /// Gets one random 12 byte array (nonce) per (<see cref="MaxValue"/>) bytes of file to encrypt.
        /// </summary>
        /// <param name="dataLength"></param>
        private List<byte[]> GetNonces( long dataLength ) {
            using Activity? activity = _source.StartActivity( "GetNonces" )?.Start( );

            List<byte[]> nonces = new( );
            int nonceCount = (int)Math.Ceiling( Convert.ToDecimal( dataLength / MaxValue ) );

            for (int i = 0; i <= nonceCount; i++) {
                GetUniqueNonce( nonces );
            }

            activity?.Stop( );
            return nonces;
        }

        internal static void GetUniqueNonce( List<byte[]> nonces ) {
            bool added = false;
            do {
                byte[]? nonce = GetNonce( );

                if (nonces.Contains( nonce ) == false) {
                    nonces.Add( nonce );
                    added = true;
                }
            } while (added == false);
        }

        /// <summary>
        /// Gets an array filled with 12 random bytes.
        /// </summary>
        private static byte[] GetNonce( ) {
            byte[] nonce = new byte[12];
            RandomNumberGenerator.Create( ).GetBytes( nonce );
            return nonce;
        }

        #endregion Encryption


        #region Decryption

        /// <summary>
        /// Decrypts <paramref name="cypherTxtFile"/> into <paramref name="plaintextFile"/> 
        /// by deserializing the <paramref name="keyFile"/> and sending it to <see cref="Decrypt"/>.
        /// </summary>
        /// <param name="keyFile"></param>
        /// <param name="cypherTxtFile"></param>
        /// <param name="plaintextFile"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task DecryptFromKeyFile(
            FileInfo keyFile,
            FileInfo cypherTxtFile,
            FileInfo plaintextFile
        ) {
            using Activity? activity = _source.StartActivity( "DecryptFromKeyFile" )?.Start( );

            // Validate Input
            if (File.Exists( keyFile.FullName ) == false) {
                activity?.Stop( );
                throw new FileNotFoundException( $"KeyFile \"{keyFile.FullName}\" doesn't exist." );
            }

            await Decrypt( ManagedChaCha20Poly1305DecryptionData.Deserialize( keyFile ), cypherTxtFile, plaintextFile );

            activity?.Stop( );
        }


        /// <summary>
        /// Uses <see cref="ChaCha20Poly1305"/> initialized with <paramref name="decryptionData"/>.Key to 
        /// decrypt <paramref name="cypherTxtFile"/> into <paramref name="plaintextFile"/>.
        /// </summary>
        /// <param name="decryptionData"></param>
        /// <param name="cypherTxtFile"></param>
        /// <param name="plaintextFile"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task Decrypt(
            ManagedChaCha20Poly1305DecryptionData decryptionData,
            FileInfo cypherTxtFile,
            FileInfo plaintextFile
        ) {
            using Activity? activity = _source.StartActivity( "Decrypt" )?.Start( );

            _log?.LogInformation( "Decrypting File '{string}'.", cypherTxtFile.FullName );

            EnsureFileExistsAndHasData( cypherTxtFile );

            await DecryptFileChunkLoop(
                plaintextFile,
                cypherTxtFile,
                new( decryptionData.KeyBytes ),
                decryptionData.KeyNoteList
            );

            _log?.LogInformation( "PlainTextFile: '{string}'.", plaintextFile.FullName );

            activity?.Stop( );
        }

        internal async Task DecryptFileChunkLoop(
            FileInfo plaintextFile,
            FileInfo cypherTxtFile,
            ChaCha20Poly1305 chaPoly,
            List<ManagedChaCha20Poly1305DecryptionKeyNote> keyNoteList,
            long processedBytes = 0,
            int chunkCount = 0
        ) {
            while (processedBytes < cypherTxtFile.Length) {

                byte[] plaintext = ((chunkCount + 1) == keyNoteList.Count) ?
                                    new byte[cypherTxtFile.Length - processedBytes] :
                                    new byte[MaxValue];

                await DecryptFileChunk(
                    chaPoly,
                    keyNoteList[chunkCount].Nonce,
                    keyNoteList[chunkCount].Tag,
                    plaintext,
                    cypherTxtFile,
                    plaintextFile,
                    processedBytes,
                    chunkCount
                );

                processedBytes += plaintext.Length;
                chunkCount++;
            }
        }

        /// <summary>
        /// Uses <paramref name="chaPoly"/> and the <see cref="ManagedChaCha20Poly1305DecryptionData"/> (<paramref name="nonce"/>,<paramref name="tag"/>)
        /// to decrypt <paramref name="cypherTxt"/>.Length bytes from <paramref name="offset"/> of <paramref name="cypherTxtFile"/> 
        /// into <paramref name="plaintextFile"/>.
        /// </summary>
        /// <param name="chaPoly"></param>
        /// <param name="nonce"></param>
        /// <param name="tag"></param>
        /// <param name="cypherTxt"></param>
        /// <param name="cypherTxtFile"></param>
        /// <param name="plaintextFile"></param>
        /// <param name="offset"></param>
        /// <param name="order"></param>
        private async Task DecryptFileChunk(
            ChaCha20Poly1305 chaPoly,
            byte[] nonce,
            byte[] tag,
            byte[] cypherTxt,
            FileInfo cypherTxtFile,
            FileInfo plaintextFile,
            long offset,
            int order
        ) {
            if (PlatformSupported == false) { throw new InvalidOperationException( "ChaCha20Poly1305 isn't supported on this platform. Cannot create managed instance." ); }
            using Activity? activity = _source.StartActivity( "DecryptFileChunk" )?.Start( );

            byte[]? plainText = new byte[cypherTxt.Length];

            // Decrypt
            chaPoly.Decrypt(
                nonce,
                await ReadFileChunkFromOffset( cypherTxtFile, cypherTxt, offset ),
                tag,
                plainText
            );

            // Append plaintext data to plaintextFile.
            await AppendFileChunk( plaintextFile, plainText, order );

            activity?.Stop( );
        }

        #endregion Decryption


        #region Helper Methods

        internal static void EnsureFileExistsAndHasData( FileInfo file ) {
            if (file.Exists == false || file.Length == 0) {
                throw new ArgumentOutOfRangeException(
                    nameof( file ),
                    $"PlainTextFile \"{file.FullName}\" must exist and have a length greater than 0."
                );
            }
        }

        /// <summary>
        /// Common method to append <paramref name="data"/> to the end of the <paramref name="outputFile"/>.
        /// <paramref name="order"/> == 0 sets <see cref="FileMode.Create"/> otherwise sets <see cref="FileMode.Open"/>.
        /// </summary>
        /// <param name="outputFile"></param>
        /// <param name="data"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private async Task AppendFileChunk(
            FileInfo outputFile,
            byte[] data,
            int order
        ) {
            using Activity? activity = _source.StartActivity( "AppendFileChunk" )?.Start( );

            FileMode mode = (order == 0) ? FileMode.Create : FileMode.Open;
            using FileStream outputFS = new(
                outputFile.FullName,
                mode,
                FileAccess.Write,
                FileShare.Read,
                10240,
                FileOptions.Asynchronous
            );
            _ = outputFS.Seek( 0, SeekOrigin.End );
            await outputFS.WriteAsync( data.AsMemory( 0, data.Length ) );

            activity?.Stop( );
        }

        /// <summary>
        /// Reads and returns <paramref name="data"/>.Length bytes from <paramref name="inputFile"/> after
        /// the <paramref name="offset"/> 
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private async Task<byte[]> ReadFileChunkFromOffset(
            FileInfo inputFile,
            byte[] data,
            long offset
        ) {
            using Activity? activity = _source.StartActivity( "ReadFileChunkFromOffset" )?.Start( );

            using FileStream inputFS = new(
                inputFile.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                10240,
                FileOptions.Asynchronous
            );
            // Read File from offset
            _ = inputFS.Seek( offset, SeekOrigin.Begin );
            _ = await inputFS.ReadAsync( data.AsMemory( 0, data.Length ) );

            activity?.Stop( );
            return data;
        }

        #endregion Helper Methods
    }
}
