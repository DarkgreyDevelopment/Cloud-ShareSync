using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        internal async Task<string> NewAuthToken( ) {
            using Activity? activity = _source.StartActivity( "NewAuthToken" )?.Start( );

            AuthReturn? authReturn = await GetBackBlazeGeneralClient( ).NewAuthReturn( _applicationData.Credentials );
            _authorizationData = authReturn.AuthData;

            activity?.Stop( );
            return authReturn.AuthToken ??
                throw new InvalidB2Response(
                    AuthorizationURI,
                    new NullReferenceException( "AuthorizationToken" )
                );
        }

    }
}
