using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        internal async Task<UploadB2File> NewStartLargeFileURL( UploadB2File uploadObject ) {
            using Activity? activity = _source.StartActivity( "NewStartLargeFileURL" )?.Start( );

            string uploadUri = _authorizationData.ApiUrl + "/b2api/v2/b2_start_large_file";
            DateTimeOffset dto = new( uploadObject.FilePath.LastWriteTimeUtc );
            byte[] data = Encoding.UTF8.GetBytes( $@"{{
  ""contentType"": ""{uploadObject.MimeType}"",
  ""bucketId"": ""{_applicationData.BucketId}"",
  ""fileName"": ""{CleanUploadPath( uploadObject.UploadFilePath, true )}"",
  ""fileInfo"": {{
    ""sha512_filehash"": ""{uploadObject.CompleteSha512Hash}"",
    ""large_file_sha1"": ""{uploadObject.CompleteSha1Hash}"",
    ""src_last_modified_millis"": ""{dto.ToUnixTimeMilliseconds( )}""
  }},
  ""serverSideEncryption"": {{
    ""mode"": ""SSE-B2"",
    ""algorithm"": ""AES256""
  }}
}}" );

            JsonElement root = await GetBackBlazeGeneralClient( ).GetJsonResponse(
                uploadUri,
                HttpMethod.Post,
                await NewAuthToken( ),
                data,
                null
            );
            _log?.LogDebug( "NewStartLargeFileURL Response: {string}", root );

            uploadObject.FileId =
                root.GetProperty( "fileId" ).GetString( ) ??
                throw new InvalidB2Response(
                    uploadUri,
                    new NullReferenceException( "FileId" )
                );

            activity?.Stop( );
            return uploadObject;
        }
    }
}
