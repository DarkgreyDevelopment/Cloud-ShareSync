using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.CloudProvider.BackBlaze.Types;
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

            ThreadProcessTime threadStats = new( thread );

            int errCount = 1;
            while (filePartQueue.IsEmpty == false) {

                filePartQueue.TryPop( out FilePartInfo? partInfo );
                if (partInfo == null) { continue; }
                concurrencyStats.StartThread( thread, partInfo.PartNumber );
                PartProcessTime partStats = new( partInfo.PartNumber );
                partStats.AddNewStartTime( );

                string pretxt = $"Thread#{thread} Part#{partInfo.PartNumber}";

                if (string.IsNullOrWhiteSpace( partInfo.Sha1Hash )) {
                    _log?.LogDebug( "{string} - Retrieving Sha1 Hash for FileChunk", pretxt );
                    partInfo.Sha1Hash = await _fileHash.GetSha1Hash(
                        upload.FilePath,
                        partInfo.Data,
                        partSize * (partInfo.PartNumber - 1)
                    );
                }

                bool success = false;
                try {
                    _log?.LogInformation(
                        "{string} Uploading LargeFile '{string}.",
                        pretxt,
                        upload.OriginalFileName
                    );
                    _log?.LogInformation( "{string} FileName      : {string}", pretxt, upload.FilePath.Name );
                    _log?.LogInformation( "{string} UploadFilePath: {string}", pretxt, upload.UploadFilePath );
                    _log?.LogInformation( "{string} PartSha1Hash  : {string}", pretxt, partInfo.Sha1Hash );
                    _log?.LogInformation( "{string} ContentSize   : {string}", pretxt, partInfo.Data.Length );

                    UploadB2FilePart uploadPart = new( threadUpload, partInfo.Sha1Hash, partInfo.PartNumber, partInfo.Data );
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
                        "{string} -  RESULTS LIST COUNT: {int}",
                        pretxt,
                        resultsList.Select( x => x.Sha1Hash ).Count( )
                    );
                    success = true;
                } catch (HttpRequestException e) {
                    filePartQueue.Push( partInfo );
                    threadStats.PartTimes.Add( partStats );
                    HandleBackBlazeException( e, errCount, thread, filePartQueue, concurrencyStats );
                    errCount++;
                    threadUpload = await NewUploadLargeFilePartUrl( upload );
                } catch (Exception ex) {
                    _log?.LogWarning( "{string} had an exception.\n{exception}", pretxt, ex );
                    filePartQueue.Push( partInfo );
                    concurrencyStats.FailThread( thread, partInfo.PartNumber );
                    partStats.AddNewStopTime( );
                    threadStats.PartTimes.Add( partStats );
                    B2ThreadManager.ThreadTimeStats.Add( threadStats );
                    activity?.Stop( );
                    return false;
                }

                partStats.AddNewStopTime( );
                threadStats.PartTimes.Add( partStats );

                if (errCount >= _applicationData.MaxErrors && success != true) {
                    _log?.LogError( "{string} hit max errors. Thread shutting down.", pretxt );
                    filePartQueue.Push( partInfo );
                    concurrencyStats.FailThread( thread, partInfo.PartNumber );
                    B2ThreadManager.ThreadTimeStats.Add( threadStats );
                    activity?.Stop( );
                    return false;
                } else if (success) {
                    errCount = 1;
                }
            }

            _log?.LogInformation( "Thread#{string} Finished Assigned Work.", thread );
            concurrencyStats.ThreadCompleted( thread );
            B2ThreadManager.ThreadTimeStats.Add( threadStats );
            activity?.Stop( );
            return true;
        }
    }
}
