using System.Collections.Concurrent;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types;
using Cloud_ShareSync.Core.CloudProvider.SharedServices;
using Cloud_ShareSync.Core.Cryptography;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2 {
    public partial class B2Api {

        public async Task DownloadFileId(
            string fileId,
            FileInfo outputPath,
            long? contentLength = null,
            string? sha512Hash = null
        ) {
            if (contentLength == null || sha512Hash == null) {
                DownloadFileById fileHeadInfo = await CallDownloadFileByIdApi(
                    fileId, HttpMethod.Head, null
                );
                contentLength = fileHeadInfo.ContentLength;
                sha512Hash = SelectSha512FileInfo( fileHeadInfo.Info );
            }

            await RunDownloadProcess( fileId, outputPath, (long)contentLength, sha512Hash );
        }

        public static string? SelectSha512FileInfo( Dictionary<string, string> fileInfo ) {
            string sha512InfoHeaderKey = "x-bz-info-sha512_filehash";
            return fileInfo.ContainsKey( sha512InfoHeaderKey )
                ? fileInfo[sha512InfoHeaderKey]
                : null;
        }

        private async Task RunDownloadProcess(
            string fileId,
            FileInfo outputPath,
            long contentLength,
            string? sha512Hash
        ) {
            using FileStream saveFile = new(
                outputPath.FullName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                10240,
                FileOptions.SequentialScan
            );
            if (contentLength < LargeFileSize) {
                await SmallFileDownload( saveFile, fileId );
            } else {
                await MultiPartFileDownload( saveFile, fileId, contentLength );
            }
            if (sha512Hash != null) { await ValidateDownloadedFileData( outputPath, sha512Hash ); }
        }

        private static async Task ValidateDownloadedFileData(
            FileInfo outputPath,
            string uploadSha512Hash
        ) {
            string downloadedSha512Hash = await new Hashing( ).GetSha512Hash( outputPath );
            if (uploadSha512Hash != downloadedSha512Hash) {
                throw new FailedB2RequestException( "Downloaded filehash does not match uploaded filehash." );
            }
        }

        private async Task<DownloadFileById> CallDownloadFileByIdApi(
            string fileId,
            HttpMethod method,
            string? range
        ) {
            DownloadFileById fileHeadInfo;
            try {
                fileHeadInfo = await DownloadFileById.CallApi(
                    method,
                    AuthToken.downloadUrl,
                    AuthToken.authorizationToken,
                    fileId,
                    range,
                    GetHttpClient( ),
                    _log
                );
            } catch (NewAuthTokenRequiredException) {
                await UpdateAuthData( );
                fileHeadInfo = await DownloadFileById.CallApi(
                    method,
                    AuthToken.downloadUrl,
                    AuthToken.authorizationToken,
                    fileId,
                    range,
                    GetHttpClient( ),
                    _log
                );
            }
            return fileHeadInfo;
        }


        #region SmallFileDownload

        private async Task SmallFileDownload(
            FileStream saveFile,
            string fileId
        ) {
            DownloadFileById downloadData = await CallDownloadFileByIdApi(
                fileId, HttpMethod.Post, null
            );
            using Stream contentStream = downloadData.Response!
                .ReadContentStream(
                    AuthToken.downloadUrl + DownloadFileById.EndpointURI,
                    EndpointCalls.DownloadFileById.ToString( )
                );
            WriteFileData( contentStream, saveFile );
        }

        private static void WriteFileData(
            Stream contentStream,
            FileStream saveFile
        ) {
            int count;
            byte[] buffer = new byte[4096];
            using BinaryReader br = new( contentStream );
            using BinaryWriter writeFile = new( saveFile );
            while ((count = br.Read( buffer, 0, buffer.Length )) != 0) {
                writeFile.Write( buffer, 0, count );
            }
        }

        #endregion SmallFileDownload


        #region LargeFileDownload

        private async Task MultiPartFileDownload(
            FileStream saveFile,
            string fileId,
            long contentLength
        ) {
            ConcurrentStack<FilePartResult> downloadPartQueue = CreatePartsQueue( contentLength );
            long partCount = downloadPartQueue.Count;
            ConcurrentBag<DownloadResultInfo> downloadedData = new( );
            DownloadFileIdParts( fileId, downloadPartQueue, downloadedData );
            await WriteMultiPartDownloadData(
                saveFile,
                partCount,
                downloadedData
            );
        }

        private ConcurrentStack<FilePartResult> CreatePartsQueue( long contentLength ) {
            ThreadArbiter arbiter = ArbitrateThreads( contentLength );
            ConcurrentStack<FilePartResult> downloadPartStack = new( );
            for (int i = arbiter.TotalParts; i >= 1; i--) {
                int partLength = i == arbiter.TotalParts ? arbiter.FinalSize : arbiter.PartSize;
                downloadPartStack.Push( new( i, partLength, (long)(i - 1) * arbiter.PartSize ) );
            }
            return downloadPartStack;
        }

        private async void DownloadFileIdParts(
            string fileId,
            ConcurrentStack<FilePartResult> filePartStack,
            ConcurrentBag<DownloadResultInfo> downloadedData
        ) {
            _log?.LogInformation( "Downloading Large File From Backblaze." );
            List<Task> downloadTasks = new( );
            int count = 1;
            for (int i = 0; i < _initData.HttpThreads; i++) {
                downloadTasks.Add(
                    DownloadLargeFileParts(
                        fileId,
                        filePartStack,
                        downloadedData,
                        count
                    )
                );
                count++;
            }

            while (downloadTasks.Any( x => x.IsCompleted == false )) {
                await Task.Delay( 1000 );
            }
        }

        private async Task DownloadLargeFileParts(
            string fileId,
            ConcurrentStack<FilePartResult> filePartStack,
            ConcurrentBag<DownloadResultInfo> downloadedData,
            int thread
        ) {
            while (filePartStack.IsEmpty == false) {
                bool dequeuedItem = filePartStack.TryPop( out FilePartResult? partData );
                if (CheckTryAction( partData, dequeuedItem )) { continue; }
                try {
                    DownloadFileById download = await CallDownloadFileByIdApi( fileId, HttpMethod.Get, GetRange( partData! ) );
                    byte[] byteArray = GetResponseBytes( download.Response! );
                    downloadedData.Add( new( byteArray, partData!.PartNumber ) );
                } catch (Exception ex) {
                    _log?.LogError(
                        "Thread#{thread} Part#{partData.PartNumber} - File part failed to download.\n{string}",
                        thread,
                        partData!.PartNumber,
                        BuildExceptionMessage( ex )
                    );
                    filePartStack.Push( partData! );
                }
            }
        }

        private static string GetRange( FilePartResult partData ) =>
            $"{partData.Offset}-{partData.Offset + partData.PartSize - 1}";

        private byte[] GetResponseBytes( HttpResponseMessage response ) {
            using MemoryStream ms = new( );
            using Stream contentStream = response.ReadContentStream(
                    AuthToken.downloadUrl + DownloadFileById.EndpointURI,
                    EndpointCalls.DownloadFileById.ToString( )
                );
            contentStream.CopyTo( ms );
            return ms.ToArray( );
        }

        private static async Task WriteMultiPartDownloadData(
            FileStream saveFile,
            long totalParts,
            ConcurrentBag<DownloadResultInfo> downloadedData
        ) {
            ConcurrentQueue<byte[]> responseQueue = new( );
            List<Task> downloadTasks = new( );
            downloadTasks.Add( PopulateResponseQueue( totalParts, downloadedData, responseQueue ) );
            downloadTasks.Add( WriteDownloadDataLoop( saveFile, totalParts, responseQueue ) );

            await Task.WhenAll( downloadTasks );
        }

        private static async Task PopulateResponseQueue(
            long totalParts,
            ConcurrentBag<DownloadResultInfo> downloadedData,
            ConcurrentQueue<byte[]> responseQueue
        ) {
            long partNum = 1;
            Dictionary<long, byte[]> byteDict = new( );
            while (partNum <= totalParts) {

                if (byteDict.ContainsKey( partNum )) {
                    responseQueue.Enqueue( byteDict[partNum] );
                    partNum += 1;
                }

                if (await PullDownloadData( partNum, byteDict, downloadedData, responseQueue )) { partNum += 1; }
            }
        }

        private static async Task<bool> PullDownloadData(
            long partNum,
            Dictionary<long, byte[]> byteDict,
            ConcurrentBag<DownloadResultInfo> downloadedData,
            ConcurrentQueue<byte[]> responseQueue
        ) {
            bool tookItem = downloadedData.TryTake( out DownloadResultInfo? downloadInfo );
            if (CheckTryAction( downloadInfo, tookItem )) {
                await Task.Delay( 500 );
            } else {
                if (downloadInfo!.PartNumber == partNum) {
                    responseQueue.Enqueue( downloadInfo.Data );
                    return true;
                } else {
                    byteDict.Add( downloadInfo.PartNumber, downloadInfo.Data );
                }
            }
            return false;
        }


        private static async Task WriteDownloadDataLoop(
            FileStream saveFile,
            long totalParts,
            ConcurrentQueue<byte[]> responseQueue
        ) {
            long partNum = 1;
            while (partNum <= totalParts) {
                bool tookItem = responseQueue.TryDequeue( out byte[]? data );
                if (CheckTryAction( data, tookItem )) {
                    await Task.Delay( 1000 );
                    continue;
                }
                foreach (byte d in data!) { saveFile.WriteByte( d ); }
                partNum++;
            }
            saveFile.Close( );
        }

        #endregion LargeFileDownload

        private static bool CheckTryAction( object? obj, bool receivedItem ) =>
            receivedItem == false || obj == null;

    }
}

