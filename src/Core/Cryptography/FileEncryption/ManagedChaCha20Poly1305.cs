﻿using System.Diagnostics;
using System.Security.Cryptography;
using Cloud_ShareSync.Core.Cryptography.FileEncryption.Types;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Cryptography.FileEncryption {
    public class ManagedChaCha20Poly1305 {

        public static bool PlatformSupported { get { return ChaCha20Poly1305.IsSupported; } }

        private readonly ActivitySource _source = new( "ManagedChaCha20Poly1305" );
        private const int MaxValue = 500000000;
        private readonly ILogger? _log;

        public ManagedChaCha20Poly1305( ILogger? log = null ) { _log = log; }

        #region Encryption

        /// <summary>
        /// Uses <see cref="ChaCha20Poly1305"/> initialized with <paramref name="key"/> to 
        /// encrypt <paramref name="plaintextFile"/> into <paramref name="cypherTxtFile"/>.
        /// DecryptionData is returned. Optionally DecryptionData can be written to <paramref name="keyFile"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="plaintextFile"></param>
        /// <param name="cypherTxtFile"></param>
        /// <param name="keyFile"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<DecryptionData> Encrypt(
            byte[] key,
            FileInfo plaintextFile,
            FileInfo cypherTxtFile,
            FileInfo? keyFile = null
        ) {
            using Activity? activity = _source.StartActivity( "Encrypt" )?.Start( );

            string encDbgStr = $"Encrypting '{plaintextFile.FullName}' into '{cypherTxtFile.FullName}'.";
            encDbgStr += keyFile == null ? "" : $" Decryption keyfile will be located at '{keyFile.FullName}'.";
            _log?.LogDebug( "{string}", encDbgStr );

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

            string enc2DbgStr = $"Encrypted '{plaintextFile.FullName}' into '{cypherTxtFile.FullName}'. ";
            enc2DbgStr += keyFile == null ? "" : $"Decryption keyfile is located at '{keyFile.FullName}'. ";
            enc2DbgStr += "Decryption Data: " + decryptionData.ToString( );
            _log?.LogDebug( "{string}", enc2DbgStr );
            activity?.Stop( );
            return decryptionData;
        }

        /// <summary>
        /// Uses <see cref="ChaCha20Poly1305"/> initialized with <paramref name="key"/> to 
        /// encrypt <paramref name="plaintextFile"/> into <paramref name="cypherTxtFile"/>.
        /// DecryptionData is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="plaintextFile"></param>
        /// <param name="cypherTxtFile"></param>
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
            int chunkCount = 0;

            while (processedBytes < plaintextFile.Length) {
                SystemMemoryChecker.Update( );
                byte[] plaintext = ((chunkCount + 1) == uniqueNonces.Count) ?
                                    new byte[plaintextFile.Length - processedBytes] :
                                    new byte[MaxValue];

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

            SystemMemoryChecker.Update( );

            activity?.Stop( );
            return new DecryptionData( key, keyNoteList );
        }

        /// <summary>
        /// Uses <paramref name="chaPoly"/> to read a <paramref name="plaintext"/>.Length section of 
        /// <paramref name="plaintextFile"/> from the <paramref name="offset"/> and encrypt it into 
        /// <paramref name="cypherTxtFile"/>.
        /// </summary>
        /// <returns><see cref="DecryptionKeyNote"/> (<paramref name="nonce"/>, tag, <paramref name="order"/>)
        /// for encrypted section.</returns>
        /// <param name="chaPoly"></param>
        /// <param name="nonce"></param>
        /// <param name="plaintext"></param>
        /// <param name="plaintextFile"></param>
        /// <param name="cypherTxtFile"></param>
        /// <param name="order"></param>
        /// <param name="offset"></param>
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
            await AppendFileChunk( cypherTxtFile, cypherTxt, order );

            activity?.Stop( );
            return new DecryptionKeyNote( nonce, tag, order );
        }

        /// <summary>
        /// Gets one random 12 byte array (nonce) per (<see cref="MaxValue"/>) of file to encrypt.
        /// </summary>
        /// <param name="dataLength"></param>
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
                throw new ArgumentException( $"KeyFile \"{keyFile.FullName}\" doesn't exist.", nameof( keyFile ) );
            }

            await Decrypt( DecryptionData.Deserialize( keyFile ), cypherTxtFile, plaintextFile );

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
            int chunkCount = 0;

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

            _log?.LogInformation( "PlainTextFile: '{string}'.", plaintextFile.FullName );

            activity?.Stop( );
        }

        /// <summary>
        /// Uses <paramref name="chaPoly"/> and the <see cref="DecryptionData"/> (<paramref name="nonce"/>,<paramref name="tag"/>)
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

        /// <summary>
        /// Common method to append <paramref name="data"/> to the end of the <paramref name="outputFile"/>.
        /// <paramref name="order"/> == 1 sets FileMode
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
            outputFS.Seek( 0, SeekOrigin.End );
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
            inputFS.Seek( offset, SeekOrigin.Begin );
            await inputFS.ReadAsync( data.AsMemory( 0, data.Length ) );

            activity?.Stop( );
            return data;
        }

        #endregion Helper Methods
    }
}
