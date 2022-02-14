using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.Core.Configuration.Types;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void ConfigureTelemetryLogger( Log4NetConfig? config ) {
            if (config == null) {
                Console.WriteLine(
                    "Log configuration is null. " +
                    "This means that Log4Net was excluded from the Cloud_ShareSync EnabledFeatures. " +
                    "Add Log4Net to the core enabledfeatures to re-enable logging."
                );
            }

            s_logger = new TelemetryLogger( s_sourceList, config );
        }

    }
}
