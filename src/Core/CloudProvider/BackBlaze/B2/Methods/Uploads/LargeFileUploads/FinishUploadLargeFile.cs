﻿using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private async Task FinishUploadLargeFile( UploadB2File uploadObject ) {
            using Activity? activity = _source.StartActivity( "FinishUploadLargeFile" )?.Start( );
            _log?.Debug( uploadObject );

            B2FinishLargeFileRequest finishLargeFileData = new( ) {
                fileId = uploadObject.FileId,
                partSha1Array = uploadObject.Sha1PartsList
                                    .OrderBy( x => x.Key )
                                    .Select( x => x.Value )
                                    .ToList( )
            };

            _log?.Debug( "finishLargeFileData: " + finishLargeFileData );

            await GetBackBlazeGeneralClient( ).SendStringContent(
                _authorizationData.ApiUrl + "/b2api/v2/b2_finish_large_file",
                HttpMethod.Post,
                await NewAuthToken( ),
                FinishLargeFileRequestToJson( finishLargeFileData )
            );

            activity?.Stop( );
        }

    }
}
