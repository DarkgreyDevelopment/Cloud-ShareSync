using Cloud_ShareSync.Core.CloudProvider.SharedServices;
using Microsoft.Extensions.DependencyInjection;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private BackBlazeHttpClient GetBackBlazeGeneralClient( ) =>
            _services.GetRequiredService<BackBlazeHttpClient>( );

    }
}
