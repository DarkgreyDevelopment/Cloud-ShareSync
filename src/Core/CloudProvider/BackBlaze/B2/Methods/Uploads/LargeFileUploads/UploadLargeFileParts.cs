using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {
        private async Task<bool> UploadLargeFileParts(
            UploadB2File upload,
            long partSize,
            ConcurrentBag<LargeFilePartReturn> resultsList,
            ConcurrentStack<FilePartInfo> filePartQueue,
            int thread,
            B2ConcurrentStats concurrencyStats
        ) {
            using Activity? activity = _source.StartActivity( "UploadLargeFileParts" )?.Start( );

            UploadB2File? threadUpload = await NewUploadLargeFilePartUrl( upload );

            int count = 1;
            while (filePartQueue.IsEmpty == false) {

                filePartQueue.TryPop( out FilePartInfo? partInfo );
                if (partInfo == null) { continue; }

                string pretxt = $"Thread#{thread} Part#{partInfo.PartNumber}";

                if (string.IsNullOrWhiteSpace( partInfo.Sha1Hash )) {
                    _log?.Debug( $"{pretxt} - Retrieving Sha1 Hash for FileChunk" );
                    partInfo.Sha1Hash = await _fileHash.GetSHA1HashForFileChunkAsync(
                        upload.FilePath,
                        partInfo.Data,
                        partSize * (partInfo.PartNumber - 1)
                    );
                }

                bool success = false;
                try {
                    _log?.Info( $"Thread#{thread} " +
                        $"Uploading LargeFile '{upload.OriginalFileName}' Part#{partInfo.PartNumber}." );
                    _log?.Info( $"{pretxt} FileName      : {upload.FilePath.Name}" );
                    _log?.Info( $"{pretxt} UploadFilePath: {upload.UploadFilePath}" );
                    _log?.Info( $"{pretxt} PartSha1Hash  : {partInfo.Sha1Hash}" );
                    _log?.Info( $"{pretxt} ContentSize   : {partInfo.Data.Length}" );

                    UploadB2FilePart uploadPart = new( threadUpload, partInfo.Sha1Hash, partInfo.PartNumber, partInfo.Data );

                    concurrencyStats.StartThread( thread );
                    await UploadLargeFilePart( uploadPart, thread );
                    concurrencyStats.ThreadActive( thread );

                    //Upload segment of file data, Adds to TotalBytesSent +Sha1PartsList
                    _log?.Info(
                        $"{pretxt}: LargeFile '{upload.OriginalFileName}' part uploaded successfully." +
                        $" Parts Sha1Hash: {uploadPart.PartSha1Hash}"
                    );
                    resultsList.Add( new( partInfo.PartNumber, uploadPart.PartSha1Hash, uploadPart.Content.Length ) );
                    _log?.Debug(
                        $"Thread#{thread} Part#{partInfo.PartNumber} -  RESULTS LIST COUNT: " +
                        resultsList.Select( x => x.Sha1Hash ).Count( )
                    );
                    success = true;
                } catch (HttpRequestException e) {
                    filePartQueue.Push( partInfo );
                    HandleBackBlazeException( e, count, thread, filePartQueue, concurrencyStats );
                    count++;
                    threadUpload = await NewUploadLargeFilePartUrl( upload );
                } catch (Exception ex) {
                    _log?.Warn( $"Thread#{thread} had an exception", ex );
                    filePartQueue.Push( partInfo );
                    concurrencyStats.FailThread( thread );
                    activity?.Stop( );
                    return false;
                }

                if (count >= _applicationData.MaxErrors && success != true) {
                    _log?.Error( $"Thread#{thread} hit max errors. Thread shutting down." );
                    filePartQueue.Push( partInfo );
                    concurrencyStats.FailThread( thread );
                    activity?.Stop( );
                    return false;
                } else if (success) {
                    count = 1;
                }
            }

            _log?.Info( $"Thread#{thread} Finished Assigned Work." );
            concurrencyStats.ThreadCompleted( thread );
            activity?.Stop( );
            return true;
        }
    }
}
