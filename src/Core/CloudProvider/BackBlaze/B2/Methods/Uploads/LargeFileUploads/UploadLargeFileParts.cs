using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Microsoft.Extensions.Logging;

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
                    _log?.LogDebug( "{string} - Retrieving Sha1 Hash for FileChunk", pretxt );
                    partInfo.Sha1Hash = await _fileHash.GetSHA1HashForFileChunkAsync(
                        upload.FilePath,
                        partInfo.Data,
                        partSize * (partInfo.PartNumber - 1)
                    );
                }

                bool success = false;
                try {
                    _log?.LogInformation(
                        "Thread#{string} Uploading LargeFile '{string}' Part#{string}.",
                        thread,
                        upload.OriginalFileName,
                        partInfo.PartNumber
                    );
                    _log?.LogInformation( "{string} FileName      : {string}", pretxt, upload.FilePath.Name );
                    _log?.LogInformation( "{string} UploadFilePath: {string}", pretxt, upload.UploadFilePath );
                    _log?.LogInformation( "{string} PartSha1Hash  : {string}", pretxt, partInfo.Sha1Hash );
                    _log?.LogInformation( "{string} ContentSize   : {string}", pretxt, partInfo.Data.Length );

                    UploadB2FilePart uploadPart = new( threadUpload, partInfo.Sha1Hash, partInfo.PartNumber, partInfo.Data );

                    concurrencyStats.StartThread( thread );
                    await UploadLargeFilePart( uploadPart, thread );
                    concurrencyStats.ThreadActive( thread );

                    //Upload segment of file data, Adds to TotalBytesSent +Sha1PartsList
                    _log?.LogInformation(
                        "{string}: LargeFile '{string}' part uploaded successfully." +
                        " Parts Sha1Hash: {string}",
                        pretxt,
                        upload.OriginalFileName,
                        uploadPart.PartSha1Hash
                    );
                    resultsList.Add( new( partInfo.PartNumber, uploadPart.PartSha1Hash, uploadPart.Content.Length ) );
                    _log?.LogDebug(
                        "Thread#{string} Part#{string} -  RESULTS LIST COUNT: {int}",
                        thread,
                        partInfo.PartNumber,
                        resultsList.Select( x => x.Sha1Hash ).Count( )
                    );
                    success = true;
                } catch (HttpRequestException e) {
                    filePartQueue.Push( partInfo );
                    HandleBackBlazeException( e, count, thread, filePartQueue, concurrencyStats );
                    count++;
                    threadUpload = await NewUploadLargeFilePartUrl( upload );
                } catch (Exception ex) {
                    _log?.LogWarning( "Thread#{string} had an exception.\n{exception}", thread, ex );
                    filePartQueue.Push( partInfo );
                    concurrencyStats.FailThread( thread );
                    activity?.Stop( );
                    return false;
                }

                if (count >= _applicationData.MaxErrors && success != true) {
                    _log?.LogError( "Thread#{string} hit max errors. Thread shutting down.", thread );
                    filePartQueue.Push( partInfo );
                    concurrencyStats.FailThread( thread );
                    activity?.Stop( );
                    return false;
                } else if (success) {
                    count = 1;
                }
            }

            _log?.LogInformation( "Thread#{string} Finished Assigned Work.", thread );
            concurrencyStats.ThreadCompleted( thread );
            activity?.Stop( );
            return true;
        }
    }
}
