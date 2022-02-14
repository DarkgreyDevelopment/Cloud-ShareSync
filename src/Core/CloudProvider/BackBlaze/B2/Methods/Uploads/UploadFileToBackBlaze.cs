using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        internal async Task<string> UploadFileToBackBlaze( UploadB2File upload ) {
            using Activity? activity = _source.StartActivity( "UploadFile" )?.Start( );

            if (File.Exists( upload.FilePath.FullName ) == false) {
                activity?.Stop( );
                throw new InvalidOperationException( "Cannot upload a file that doesn't exist." );
            }

            if (string.IsNullOrWhiteSpace( upload.CompleteSha512Hash )) {
                upload.CompleteSha512Hash = await _fileHash.GetSha512Hash( upload.FilePath );
            }
            upload.CompleteSha1Hash = await _fileHash.GetSha1Hash( upload.FilePath );
            upload.MimeType = MimeType.GetMimeTypeByExtension( upload.FilePath );

            int recSize = RecommendedPartSize ?? 0;
            int minimumLargeFileSize = (recSize * 2) + AbsoluteMinimumPartSize ?? 0;

            if (minimumLargeFileSize == 0) {
                activity?.Stop( );
                throw new InvalidOperationException( "Received an invalid response from BackBlaze." );
            }
            bool smallFileUpload = upload.FilePath.Length < minimumLargeFileSize;

            if (smallFileUpload) {
                _log?.LogDebug( "Getting Small File Upload Url" );
                upload = await NewSmallFileUploadUrl( upload );
                _log?.LogDebug( "{string}", upload );

                _log?.LogDebug( "Uploading Small File to Backblaze" );
                upload = await NewSmallFileUpload( upload );
            } else /* Large File Upload */ {
                _log?.LogDebug( "Getting FileId For Large File." );
                upload = await NewStartLargeFileURL( upload );
                _log?.LogDebug( "{string}", upload );

                _log?.LogInformation( "Uploading Large File to Backblaze." );
                upload = await NewLargeFileUpload( upload );
            }
            activity?.Stop( );
            return upload.FileId;
        }

    }
}
