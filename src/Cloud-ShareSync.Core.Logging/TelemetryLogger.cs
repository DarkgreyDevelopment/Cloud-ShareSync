using System.Reflection;
using Cloud_ShareSync.Core.Logging.Appenders;
using Cloud_ShareSync.Core.Logging.Logger;
using Cloud_ShareSync.Core.Logging.Telemetry;
using log4net;
using log4net.Config;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Cloud_ShareSync.Core.Logging {

    public class TelemetryLogger : CloudShareSyncILogger {

        #region CTors

        /// <summary>
        /// Creates an <see cref="CloudShareSyncILogger"/> based ILogger implementation
        /// for the provided <paramref name="serviceName"/>.
        /// </summary>
        public TelemetryLogger( string serviceName ) : base( serviceName ) {
            Hierarchy? hierarchy = LoggerRepository as Hierarchy;
            hierarchy?.LevelMap.Add( TelemetryLogLevelExtension.TelemetryLevel );
        }


        /// <summary>
        /// Configures the OpenTelemetry settings for the provided <paramref name="sources"/> and
        /// passes the <paramref name="serviceName"/> to <see cref="TelemetryLogger(string)"/>.
        /// </summary>
        public TelemetryLogger( string serviceName, string[] sources ) : this( serviceName ) {
            OpenTelemetry = ConfigureOpenTelemetrySources( sources, serviceName );
        }

        #endregion CTors


        #region Fields

        /// <summary>
        /// The <see cref="ILoggerRepository"/> used as a <see cref="Hierarchy"/> to store
        /// Log4Net appenders. 
        /// </summary>
        private ILoggerRepository LoggerRepository { get; } =
            LogManager.GetRepository( Assembly.GetEntryAssembly( ) );


        /// <summary>
        /// The <see cref="TracerProvider"/> that will collect activities for the sources provided in
        /// <see cref="TelemetryLogger(string,string[])"/>.
        /// </summary>
        public readonly TracerProvider? OpenTelemetry;

        #endregion Fields


        #region Methods

        public void LogTelemetry( string message ) => Log4NetLog?.Telemetry( message );

        /// <summary>
        /// Used to configure <see cref="OpenTelemetry"/> with the <paramref name="serviceName"/>
        /// and <paramref name="sources"/> provided in <see cref="TelemetryLogger(string,string[])"/>.
        /// <para>Telemetry is exported via the <see cref="TelemetryExporter"/>.</para>
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="serviceName"></param>
        /// <returns>A configured <see cref="TracerProvider"/>.</returns>
        private TracerProvider ConfigureOpenTelemetrySources( string[] sources, string serviceName ) {
            return Sdk.CreateTracerProviderBuilder( )
                .AddSource( sources )
                .SetResourceBuilder( ResourceBuilder.CreateDefault( )
                .AddService( serviceName ) )
                .AddProcessor( new SimpleActivityExportProcessor( new TelemetryExporter( Log4NetLog ) ) )
                .AddLogExporter( Log4NetLog )
                .Build( );
        }


        /// <summary>
        /// Converts the <see cref="LoggerRepository"/> into a <see cref="Hierarchy"/>.
        /// Then checks <see cref="UseColoredConsoleAppenders(bool)"/>.
        /// <para>
        /// If <see cref="UseColoredConsoleAppenders(bool)"/> is true it then calls
        /// <see cref="AddColoredConsoleLogAppender"/> and <see cref="AddColoredConsoleErrorLogAppender"/>.
        /// </para>
        /// If <see cref="UseColoredConsoleAppenders(bool)"/> is false it then calls
        ///<see cref="AddConsoleLogAppender"/> and <see cref="AddConsoleErrorLogAppender"/> instead.
        /// </summary>
        public void AddConsoleAppender(
            bool useStdErr,
            SupportedLogLevels logLevels,
            bool enableColoredConsole = true
        ) {
            Hierarchy? hierarchy = LoggerRepository as Hierarchy;
            if (UseColoredConsoleAppenders( enableColoredConsole )) {
                AddColoredConsoleLogAppender( hierarchy, useStdErr, logLevels );
                AddColoredConsoleErrorLogAppender( hierarchy, useStdErr, logLevels );
            } else {
                AddConsoleLogAppender( hierarchy, useStdErr, logLevels );
                AddConsoleErrorLogAppender( hierarchy, useStdErr, logLevels );
            }
        }


        /// <summary>
        /// Checks that <paramref name="enableColoredConsole"/> is true and that 
        /// $Env:NO_COLOR does not exist.
        /// </summary>
        /// <param name="enableColoredConsole"></param>
        /// <returns>
        /// True if <paramref name="enableColoredConsole"/> is true and $Env:NO_COLOR is not set.
        /// Otherwise returns false.
        /// </returns>
        private static bool UseColoredConsoleAppenders( bool enableColoredConsole ) =>
            enableColoredConsole && Environment.GetEnvironmentVariable( "NO_COLOR" ) == null;


        /// <summary>
        /// Adds a <see cref="ColoredConsoleLogAppender(SupportedLogLevels,bool)"/> to the
        /// <paramref name="hierarchy"/>.Root
        /// <para>If <paramref name="useStdErr"/> is false then
        /// <see cref="ColoredConsoleLogAppender(SupportedLogLevels, bool)"/> addErrorLevels is true."
        /// </para>
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="useStdErr"></param>
        /// <param name="logLevels"></param>
        private static void AddColoredConsoleLogAppender(
            Hierarchy? hierarchy,
            bool useStdErr,
            SupportedLogLevels logLevels
        ) => hierarchy?.Root.AddAppender( new ColoredConsoleLogAppender( logLevels, useStdErr == false ) );


        /// <summary>
        /// If <paramref name="useStdErr"/> is true then 
        /// adds a <see cref="ColoredConsoleErrorLogAppender(SupportedLogLevels)"/> to the
        /// <paramref name="hierarchy"/>.Root
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="useStdErr"></param>
        /// <param name="logLevels"></param>
        private static void AddColoredConsoleErrorLogAppender(
            Hierarchy? hierarchy,
            bool useStdErr,
            SupportedLogLevels logLevels
        ) {
            if (useStdErr) {
                hierarchy?.Root
                    .AddAppender(
                        new ColoredConsoleErrorLogAppender( logLevels )
                    );
            };
        }


        /// <summary>
        /// Adds a <see cref="ConsoleLogAppender(SupportedLogLevels,bool)"/> to the
        /// <paramref name="hierarchy"/>.Root
        /// <para>If <paramref name="useStdErr"/> is false then
        /// <see cref="ConsoleLogAppender(SupportedLogLevels, bool)"/> addErrorLevels is true."
        /// </para>
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="useStdErr"></param>
        /// <param name="logLevels"></param>
        private static void AddConsoleLogAppender(
            Hierarchy? hierarchy,
            bool useStdErr,
            SupportedLogLevels logLevels
        ) => hierarchy?.Root.AddAppender( new ConsoleLogAppender( logLevels, useStdErr == false ) );


        /// <summary>
        /// If <paramref name="useStdErr"/> is true then 
        /// adds a <see cref="ConsoleErrorLogAppender(SupportedLogLevels)"/> to the
        /// <paramref name="hierarchy"/>.Root
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="useStdErr"></param>
        /// <param name="logLevels"></param>
        private static void AddConsoleErrorLogAppender(
            Hierarchy? hierarchy,
            bool useStdErr,
            SupportedLogLevels logLevels
        ) {
            if (useStdErr) {
                hierarchy?.Root
                    .AddAppender(
                        new ConsoleErrorLogAppender( logLevels )
                    );
            };
        }


        /// <summary>
        /// Converts the <see cref="LoggerRepository"/> into a <see cref="Hierarchy"/>.
        /// Then adds a <see cref="RollingLogAppender(string,int,int,SupportedLogLevels)"/> to the 
        /// hierarchy.Root
        /// </summary>
        /// <param name="path"></param>
        /// <param name="maxSizeRollBackups"></param>
        /// <param name="maximumFileSize"></param>
        /// <param name="logLevels"></param>
        public void AddRollingLogAppender(
            string path,
            int maxSizeRollBackups,
            int maximumFileSize,
            SupportedLogLevels logLevels
        ) {
            Hierarchy? hierarchy = LoggerRepository as Hierarchy;
            hierarchy?.Root
                .AddAppender(
                    new RollingLogAppender(
                        path,
                        maxSizeRollBackups,
                        maximumFileSize,
                        logLevels
                    )
                );
        }


        /// <summary>
        /// If <see cref="OpenTelemetry"/> is configured via <see cref="TelemetryLogger(string,string[])"/>
        /// then this converts the <see cref="LoggerRepository"/> into a <see cref="Hierarchy"/> and adds a
        /// <see cref="TelemetryLogAppender(string,int,int)"/> to the hierarchy.Root
        /// <para>Otherwise does nothing.</para>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="maxSizeRollBackups"></param>
        /// <param name="maximumFileSize"></param>
        public void AddTelemetryAppender(
            string path,
            int maxSizeRollBackups,
            int maximumFileSize
        ) {
            if (OpenTelemetry == null) {
                string message = "Unable to add TelemetryAppender when OpenTelemetry is not configured.";
                Log4NetLog?.Info( message );
                Console.WriteLine( message );
                return;
            }
            Hierarchy? hierarchy = LoggerRepository as Hierarchy;
            hierarchy?.Root
                .AddAppender(
                    new TelemetryLogAppender(
                        path,
                        maxSizeRollBackups,
                        maximumFileSize
                    )
                );
        }


        /// <summary>
        /// Ensures the <paramref name="configFile"/> exists then uses
        /// <seealso cref="XmlConfigurator.Configure(ILoggerRepository,FileInfo)"/> to configure the
        /// <see cref="LoggerRepository"/>.
        /// </summary>
        /// <param name="configFile"></param>
        public void ConfigureFromLog4NetXmlConfigFile( FileInfo configFile ) {
            if (File.Exists( configFile.FullName ) == false) {
                throw new FileNotFoundException(
                    $"Log4Net ConfigurationFile '{configFile.FullName}' doesn't exist."
                );
            }

            _ = XmlConfigurator.Configure(
                LoggerRepository,
                configFile
            );
        }


        /// <summary>
        /// Converts the <see cref="LoggerRepository"/> into a <see cref="Hierarchy"/> and sets the 
        /// hierarchy.Configured property to true if it exists.
        /// </summary>
        public void SetConfigured( ) {
            Hierarchy? hierarchy = LoggerRepository as Hierarchy;
            if (hierarchy?.Configured != null) { hierarchy.Configured = true; }
        }

        #endregion Methods

    }
}
