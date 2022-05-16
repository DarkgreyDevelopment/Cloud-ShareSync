using Cloud_ShareSync.Core.Logging.Appenders.Extensions;
using log4net.Appender;
using log4net.Layout;

namespace Cloud_ShareSync.Core.Logging.Appenders {
    internal class RollingLogAppender : RollingFileAppender {

        public RollingLogAppender(
            string path,
            int maxSizeRollBackups,
            int maximumFileSize,
            SupportedLogLevels logLevels
        ) {
            AppendToFile = true;
            PreserveLogFileNameExtension = true;
            RollingStyle = RollingMode.Composite;
            StaticLogFileName = true;
            CountDirection = 1;
            File = path;
            Layout = new PatternLayout( ).DefaultPatternLayout( );
            MaxSizeRollBackups = maxSizeRollBackups;
            MaximumFileSize = maximumFileSize + "MB";
            this.AddFilters(
                logLevels
                    .TranslateLogLevel( true )
                    .CreateFiltersList( true )
            );
            ActivateOptions( );
        }
    }
}
