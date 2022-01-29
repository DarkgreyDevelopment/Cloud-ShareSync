using System.Diagnostics;
using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.SharedServices;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        internal async Task<UploadB2File> NewSmallFileUpload( UploadB2File uploadObject ) {
            using Activity? activity = _source.StartActivity( "NewSmallFileUpload" )?.Start( );

            string dto = new DateTimeOffset( uploadObject.FilePath.LastWriteTimeUtc )
                            .ToUnixTimeMilliseconds( )
                            .ToString( );

            byte[] data = File.ReadAllBytes( uploadObject.FilePath.FullName );

            /* Need to move this stuff to the client/ClientUtilities like the rest of the process. */
            /* Theres a bug that breaks things currently though so thats why this is separated. */
            // Create Initial Request
            HttpRequestMessage request = new( HttpMethod.Post, uploadObject.UploadUrl );
            // Add Authorization Headers
            request.Headers.TryAddWithoutValidation( "Authorization", uploadObject.AuthorizationToken );
            // Add UserAgent Headers
            request.Headers.TryAddWithoutValidation( "UserAgent", UserAgent );
            // Create request content
            request.Content = new ByteArrayContent( data );
            // Set request Content-Type
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( uploadObject.MimeType );
            // Set request Content-Length
            request.Content.Headers.ContentLength = data.Length;

            // Add additional content headers.
            request.Content.Headers.Add( "X-Bz-File-Name", CleanUploadPath( uploadObject.UploadFilePath, false ) );
            request.Content.Headers.Add( "X-Bz-Content-Sha1", uploadObject.CompleteSha1Hash );
            request.Content.Headers.Add( "X-Bz-Info-Author", "Cloud-ShareSync" );
            request.Content.Headers.Add( "X-Bz-Server-Side-Encryption", "AES256" );
            request.Content.Headers.Add( "X-Bz-Info-src_last_modified_millis", dto );
            request.Content.Headers.Add( "X-Bz-Info-sha512_filehash", uploadObject.CompleteSha512Hash );

            _log?.Debug( "NewSmallFileUpload Request: " + request.ToString( ) );

            SystemMemoryChecker.Update( );

            JsonElement root = await GetBackBlazeGeneralClient( ).GetJsonResponse( request );

            // Set UploadObject values.
            uploadObject.FileId = root.GetProperty( "fileId" ).GetString( ) ??
                throw new InvalidB2Response(
                    uploadObject.UploadUrl,
                    new NullReferenceException( "fileId" )
                );
            uploadObject.TotalBytesSent = data.Length;
            uploadObject.Sha1PartsList.Add( new( 0, uploadObject.CompleteSha1Hash ) );

            SystemMemoryChecker.Update( );

            activity?.Stop( );
            return uploadObject;
        }

    }
}
