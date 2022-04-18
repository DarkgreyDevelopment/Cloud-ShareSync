using Cloud_ShareSync.Core.Logging.Appenders.Extensions;
using Cloud_ShareSync.Core.Logging.Telemetry;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;

namespace Cloud_ShareSync.Core.Logging.Appenders {
    internal class TelemetryLogAppender : RollingFileAppender {

        private readonly PatternLayout _pattern = new( ) { ConversionPattern = "%m%n" };
        private static readonly List<IFilter> s_telemetryFilter =
            new Level[] { TelemetryLogLevelExtension.TelemetryLevel }.CreateFiltersList( false );

        public TelemetryLogAppender(
            string path,
            int maxSizeRollBackups,
            int maximumFileSize
        ) {
            _pattern.ActivateOptions( );
            AppendToFile = true;
            PreserveLogFileNameExtension = true;
            RollingStyle = RollingMode.Composite;
            StaticLogFileName = true;
            CountDirection = 1;
            File = path;
            Layout = _pattern;
            MaxSizeRollBackups = maxSizeRollBackups;
            MaximumFileSize = maximumFileSize + "MB";
            this.AddFilters( s_telemetryFilter );
            ActivateOptions( );
        }
    }
}
