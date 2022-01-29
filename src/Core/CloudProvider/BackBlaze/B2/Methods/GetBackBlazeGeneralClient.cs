using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.DependencyInjection;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze {

    internal partial class B2 {

        private BackBlazeHttpClient GetBackBlazeGeneralClient( ) =>
            _services.Services.GetRequiredService<BackBlazeHttpClient>( );

    }
}
