using System.Diagnostics;
using System.Security.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption.Types;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Cryptography.FileEncryption {
    public class ManagedChaCha20Poly1305 {

        public static bool PlatformSupported { get { return ChaCha20Poly1305.IsSupported; } }

        private readonly ActivitySource _source = new( "ManagedChaCha20Poly1305" );
        private const int MaxValue = 2147483591;
        private readonly ILogger? _log;

        public ManagedChaCha20Poly1305( ILogger? log = null ) { _log = log; }

        #region Encryption

        public async Task<DecryptionData> Encrypt(
            byte[] key,
            FileInfo plaintextFile,
            FileInfo cypherTxtFile,
            FileInfo? keyFile = null
        ) {
            using Activity? activity = _source.StartActivity( "Encrypt" )?.Start( );

            if (plaintextFile.Exists == false || plaintextFile.Length == 0) {
                activity?.Stop( );
                throw new ArgumentOutOfRangeException(
                    nameof( plaintextFile ),
                    $"PlainTextFile \"{plaintextFile.FullName}\" must exist and have a length greater than 0."
                );
            }

            DecryptionData decryptionData = await EncryptFile( key, plaintextFile, cypherTxtFile );

            if (keyFile != null) {
                await File.WriteAllTextAsync(
                    keyFile.FullName,
                    decryptionData.ToString( )
                );
            }

            activity?.Stop( );
            return decryptionData;
        }

        private async Task<DecryptionData> EncryptFile(
            byte[] key,
            FileInfo plaintextFile,
            FileInfo cypherTxtFile
        ) {
            using Activity? activity = _source.StartActivity( "EncryptFile" )?.Start( );

            ChaCha20Poly1305 chaPoly = new( key );
            List<byte[]> uniqueNonces = GetNonces( plaintextFile.Length );
            List<DecryptionKeyNote> keyNoteList = new( );
            long processedBytes = 0;
            int chunkCount = 1;

            _log?.LogInformation( "Encrypting File '{string}'.", plaintextFile.FullName );

            while (processedBytes < plaintextFile.Length) {

                SystemMemoryChecker.Update( );

                byte[] plaintext = (chunkCount == uniqueNonces.Count) ?
                                    new byte[plaintextFile.Length - processedBytes] :
                                    new byte[MaxValue];

                keyNoteList.Add(
                    await EncryptFileChunk(
                        chaPoly,
                        uniqueNonces[(chunkCount - 1)],
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

            SystemMemoryChecker.Update( );

            _log?.LogInformation( "CypherTextFile: '{string}'.", cypherTxtFile.FullName );

            activity?.Stop( );
            return new DecryptionData( key, keyNoteList );
        }

        private async Task<DecryptionKeyNote> EncryptFileChunk(
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
            await AppendFileChunk( cypherTxtFile, cypherTxt, offset, order );

            activity?.Stop( );
            return new DecryptionKeyNote( nonce, tag, order );
        }

        private List<byte[]> GetNonces( long dataLength ) {
            using Activity? activity = _source.StartActivity( "GetNonces" )?.Start( );

            List<byte[]> nonces = new( );
            int nonceCount = (int)Math.Ceiling( Convert.ToDecimal( dataLength / MaxValue ) );

            for (int i = 0; i <= nonceCount; i++) {
                bool added = false;
                do {
                    byte[]? nonce = GetNonce( );

                    if (nonces.Contains( nonce ) == false) {
                        nonces.Add( nonce );
                        added = true;
                    }
                } while (added == false);
            }

            activity?.Stop( );
            return nonces;
        }

        private static byte[] GetNonce( ) {
            byte[]? nonce = new byte[12];
            RandomNumberGenerator.Create( ).GetBytes( nonce );
            return nonce;
        }

        #endregion Encryption


        #region Decryption

        public async Task DecryptFromKeyFile(
            FileInfo keyFile,
            FileInfo cypherTxtFile,
            FileInfo plaintextFile
        ) {
            using Activity? activity = _source.StartActivity( "DecryptFromKeyFile" )?.Start( );

            // Validate Input
            if (File.Exists( keyFile.FullName ) == false) {
                activity?.Stop( );
                throw new ArgumentException( $"KeyFile \"{keyFile.FullName}\" doesn't exist.", nameof( keyFile ) );
            }

            await Decrypt( DecryptionData.Deserialize( keyFile ), cypherTxtFile, plaintextFile );

            activity?.Stop( );
        }

        public async Task Decrypt(
            DecryptionData decryptionData,
            FileInfo cypherTxtFile,
            FileInfo plaintextFile
        ) {
            using Activity? activity = _source.StartActivity( "Decrypt" )?.Start( );

            _log?.LogInformation( "Decrypting File '{string}'.", cypherTxtFile.FullName );

            if (cypherTxtFile.Exists == false) {
                activity?.Stop( );
                throw new ArgumentException(
                    $"CypherTxtFile \"{cypherTxtFile.FullName}\" doesn't exist. Nothing to decrypt",
                    nameof( cypherTxtFile )
                );
            } else if (cypherTxtFile.Length == 0) {
                activity?.Stop( );
                throw new ArgumentOutOfRangeException(
                    nameof( cypherTxtFile ),
                    $"cypherTxtFile \"{cypherTxtFile.FullName}\" must have a length greater than 0."
                );
            }

            ChaCha20Poly1305 chaPoly = new( decryptionData.KeyBytes );
            List<DecryptionKeyNote> keyNoteList = decryptionData.KeyNoteList;
            long processedBytes = 0;
            int chunkCount = 1;

            while (processedBytes < cypherTxtFile.Length) {

                byte[] plaintext = (chunkCount == keyNoteList.Count) ?
                                    new byte[cypherTxtFile.Length - processedBytes] :
                                    new byte[MaxValue];

                await DecryptFileChunk(
                    chaPoly,
                    keyNoteList[(chunkCount - 1)].Nonce,
                    keyNoteList[(chunkCount - 1)].Tag,
                    plaintext,
                    cypherTxtFile,
                    plaintextFile,
                    processedBytes,
                    chunkCount
                );

                processedBytes += plaintext.Length;
                chunkCount++;
            }

            _log?.LogInformation( "PlainTextFile: '{string}'.", plaintextFile.FullName );

            activity?.Stop( );
        }

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
            await AppendFileChunk( plaintextFile, plainText, offset, order );

            activity?.Stop( );
        }

        #endregion Decryption


        #region Helper Methods

        private async Task AppendFileChunk(
            FileInfo outputFile,
            byte[] data,
            long offset,
            int order
        ) {
            using Activity? activity = _source.StartActivity( "AppendFileChunk" )?.Start( );

            FileMode fileMode = (order == 1) ?
                            FileMode.Create :
                            FileMode.Open;

            using FileStream outputFS = new(
                outputFile.FullName,
                fileMode,
                FileAccess.Write,
                FileShare.Read,
                10240,
                FileOptions.Asynchronous
            );
            outputFS.Seek( offset, SeekOrigin.Current );
            await outputFS.WriteAsync( data.AsMemory( 0, data.Length ) );

            activity?.Stop( );
        }

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
            inputFS.Seek( offset, SeekOrigin.Begin );
            await inputFS.ReadAsync( data.AsMemory( 0, data.Length ) );

            activity?.Stop( );
            return data;
        }

        #endregion Helper Methods
    }
}
