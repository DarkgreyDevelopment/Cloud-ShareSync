using System.Reflection;
using System.Text;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Configuration.Types.Logging;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Cloud_ShareSync.Core.Logging {

    internal class TelemetryLogger : Microsoft.Extensions.Logging.ILogger {

        private const string ServiceName = "Cloud-ShareSync";
        private const string LogMessageFormat =
            "%utcdate{yyyy-MM-dd}T%utcdate{HH:mm:ss.ffff}Z [%thread] %5level: %m%n";
        private readonly ILoggerRepository _loggerRepository;

        public readonly ILog? ILog;
        public readonly TracerProvider? OpenTelemetry;

        public TelemetryLogger(
            string[]? sources = null,
            Log4NetConfig? config = null
        ) {
            _loggerRepository = LogManager.GetRepository( Assembly.GetEntryAssembly( ) );
            try {
                if (config != null) {
                    ILog = LogManager.GetLogger( ServiceName );

                    if (string.IsNullOrWhiteSpace( config.ConfigurationFile )) {

                        Hierarchy hierarchy = (Hierarchy)_loggerRepository;
                        hierarchy.LevelMap.Add( TelemetryLogLevelExtension.TelemetryLevel );

                        // Setup Default Rolling Log File
                        try {
                            if (config.EnableDefaultLog) { ConfigureDefaultLogAppender( hierarchy, config ); }
                        } catch (ApplicationException defaultLogFailure) {
                            string? message =
                                "Failed to add default Log4Net RollingLogAppender. Logging may be limited.";
                            Console.Error.WriteLine( message + "\n" + defaultLogFailure.ToString( ) );
                            // try logging to any configured appenders.
                            ILog?.Error( message, defaultLogFailure );
                        }

                        // Setup Telemetry Log File
                        try {
                            if (config.EnableTelemetryLog) { ConfigureTelemetryAppender( hierarchy, config ); }
                        } catch (ApplicationException telemetryLogFailure) {
                            string? message =
                                "Failed to add telemetry Log4Net RollingLogAppender. Telemetry logging may be limited.";
                            Console.Error.WriteLine( message + "\n" + telemetryLogFailure.ToString( ) );
                            // try logging to any configured appenders.
                            ILog?.Error( message, telemetryLogFailure );
                        }

                        // Setup Console Output
                        string? consoleMessage =
                                "Failed to add Log4Net ColoredConsoleAppender. Console logging will be limited.";
                        try {
                            if (config.EnableConsoleLog) { ConfigureConsoleAppender( hierarchy, config ); }
                        } catch (DllNotFoundException dllEx) {
                            Console.Error.WriteLine( consoleMessage + "\n" + dllEx.ToString( ) );
                            // try logging to any configured appenders.
                            ILog?.Error( consoleMessage, dllEx );
                        } catch (ApplicationException consoleLogFailure) {
                            Console.Error.WriteLine( consoleMessage + "\n" + consoleLogFailure.ToString( ) );
                            // try logging to any configured appenders.
                            ILog?.Error( consoleMessage, consoleLogFailure );
                        }

                        hierarchy.Configured = true;

                    } else /* log4NetConfig.ConfigurationFile has content */ {

                        if (File.Exists( config.ConfigurationFile ) == false) {
                            throw new FileNotFoundException(
                                $"Cannot find Log4Net ConfigurationFile at {config.ConfigurationFile}"
                            );
                        }

                        _ = XmlConfigurator.Configure(
                            _loggerRepository,
                            new FileInfo( config.ConfigurationFile )
                        );

                    }
                }
            } catch (Exception ex) {
                Console.Error.WriteLine(
                    "\nAn error has occurred - Failed to configure TelemetryLogger. " +
                    "The application will be unable to log any additional messages.\n" +
                    $"Error: {ex.Message}\n" +
                    $"{ex}\n"
                );
            }

            if (sources != null) {
                // Configure OpenTelemetry
                OpenTelemetry = Sdk.CreateTracerProviderBuilder( )
                    .AddSource( sources )
                    .SetResourceBuilder( ResourceBuilder.CreateDefault( )
                    .AddService( ServiceName ) )
                    .AddProcessor( new SimpleActivityExportProcessor( new LogExporter( ILog ) ) )
                    .AddLogExporter( ILog )
                    .Build( );
            }
        }

        #region CreateAppenders

        private static void ConfigureDefaultLogAppender(
            Hierarchy hierarchy,
            Log4NetConfig log4NetConfig
        ) {

            if (log4NetConfig.DefaultLogConfiguration == null) {
                throw new ApplicationException( $"Cannot Enable Default Log if Log Config Is Null." );
            }

            DefaultLogConfig logConfig = log4NetConfig.DefaultLogConfiguration;

            PatternLayout patternLayout = new( ) { ConversionPattern = LogMessageFormat };
            patternLayout.ActivateOptions( );

            // Create RollingFileAppender Implementation
            RollingFileAppender roller = NewRollingFileAppender(
                Path.Join( logConfig.LogDirectory, logConfig.FileName ),
                patternLayout,
                logConfig.RolloverCount,
                logConfig.MaximumSize + "MB"
            );

            Level[] levels = TranslateLogLevel( logConfig.LogLevels );

            foreach (log4net.Filter.IFilter filter in CreateFiltersList( levels, false )) {
                roller.AddFilter( filter );
            }

            roller.ActivateOptions( );
            hierarchy.Root.AddAppender( roller );
        }

        private static void ConfigureTelemetryAppender(
            Hierarchy hierarchy,
            Log4NetConfig log4NetConfig
        ) {

            if (log4NetConfig.TelemetryLogConfiguration == null) {
                throw new ApplicationException( $"Cannot Enable Telemetry Log if TelemetryConfig Is Null." );
            }

            TelemetryLogConfig telemetryConfig = log4NetConfig.TelemetryLogConfiguration;

            PatternLayout patternLayout = new( ) { ConversionPattern = "%m%n" };
            patternLayout.ActivateOptions( );

            // Create RollingFileAppender Implementation.
            RollingFileAppender telemetry = NewRollingFileAppender(
                Path.Join( telemetryConfig.LogDirectory, telemetryConfig.FileName ),
                patternLayout,
                telemetryConfig.RolloverCount,
                telemetryConfig.MaximumSize + "MB"
            );

            Level[] telemetryLevel = new Level[] { TelemetryLogLevelExtension.TelemetryLevel };

            foreach (log4net.Filter.IFilter filter in CreateFiltersList( telemetryLevel, false )) {
                telemetry.AddFilter( filter );
            }

            telemetry.ActivateOptions( );
            hierarchy.Root.AddAppender( telemetry );
        }

        private static void ConfigureErrorConsoleAppender(
            Hierarchy hierarchy,
            PatternLayout pattern
        ) {

            ColoredConsoleAppender consoleErrorAppender = new( ) {
                Layout = pattern,
                Target = "Console.Error"
            };

            consoleErrorAppender.AddFilter(
                new log4net.Filter.LevelMatchFilter {
                    LevelToMatch = Level.Fatal,
                    AcceptOnMatch = true
                }
            );
            consoleErrorAppender.AddFilter(
                new log4net.Filter.LevelMatchFilter {
                    LevelToMatch = Level.Error,
                    AcceptOnMatch = true
                }
            );

            Level[] errLvl = new Level[] { Level.Fatal, Level.Error };
            foreach (ColoredConsoleAppender.LevelColors mapping in CreateMappingsList( errLvl )) {
                consoleErrorAppender.AddMapping( mapping );
            }

            log4net.Filter.DenyAllFilter denyfilter = new( );
            consoleErrorAppender.AddFilter( denyfilter );

            consoleErrorAppender.ActivateOptions( );
            hierarchy.Root.AddAppender( consoleErrorAppender );

        }

        private static void ConfigureConsoleAppender(
            Hierarchy hierarchy,
            Log4NetConfig log4NetConfig
        ) {

            if (log4NetConfig.ConsoleConfiguration == null) {
                throw new ApplicationException( $"Cannot Enable Console Log if ConsoleConfig Is Null." );
            }

            ConsoleLogConfig consoleConfig = log4NetConfig.ConsoleConfiguration;

            // Register terminal code page for console output.
            Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );

            PatternLayout patternLayout = new( ) { ConversionPattern = LogMessageFormat };
            patternLayout.ActivateOptions( );

            ColoredConsoleAppender consoleAppender = new( ) { Layout = patternLayout };

            if (consoleConfig.UseStdErr) { ConfigureErrorConsoleAppender( hierarchy, patternLayout ); }

            Level[] levels = TranslateLogLevel( consoleConfig.LogLevels );

            // Add Filters.
            List<log4net.Filter.IFilter> filters = CreateFiltersList( levels, consoleConfig.UseStdErr );

            foreach (log4net.Filter.IFilter filter in filters) {
                consoleAppender.AddFilter( filter );
            }

            // Add Mappings.
            List<ColoredConsoleAppender.LevelColors>? mappings =
                CreateMappingsList( levels );

            foreach (ColoredConsoleAppender.LevelColors mapping in mappings) {
                consoleAppender.AddMapping( mapping );
            }

            consoleAppender.ActivateOptions( );
            hierarchy.Root.AddAppender( consoleAppender );

        }

        #endregion CreateAppenders


        #region HelperMethods

        private static List<log4net.Filter.IFilter> CreateFiltersList(
            Level[] requestedLogLevels,
            bool skipErrorLevels = false
        ) {
            // Create an Accept LevelMatchFilter for all specified LogLevels.
            List<log4net.Filter.IFilter> filterList = new( );
            foreach (Level level in requestedLogLevels) {
                if (skipErrorLevels && (level == Level.Fatal || level == Level.Error)) { continue; }
                filterList.Add(
                    new log4net.Filter.LevelMatchFilter {
                        LevelToMatch = level,
                        AcceptOnMatch = true
                    }
                );
            }

            // Create Default Deny Filter to exclude all non-specified LogLevels.
            filterList.Add( new log4net.Filter.DenyAllFilter( ) );
            return filterList;
        }

        private static List<ColoredConsoleAppender.LevelColors> CreateMappingsList( Level[] requestedLogLevels ) {
            List<ColoredConsoleAppender.LevelColors> map = new( );
            foreach (Level level in requestedLogLevels) {
                GetFatalColor( level, map );
                GetErrorColor( level, map );
                GetWarnColor( level, map );
                GetInfoColor( level, map );
                GetDebugColor( level, map );
                GetTelemetryColor( level, map );
            }
            return map;
        }

        private static void GetFatalColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == Level.Fatal) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.White,
                        BackColor = ColoredConsoleAppender.Colors.Red,
                        Level = Level.Fatal
                    }
                );
            }
        }

        private static void GetErrorColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == Level.Error) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.Red,
                        Level = Level.Error
                    }
                );
            }
        }

        private static void GetWarnColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == Level.Warn) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.Yellow,
                        Level = Level.Warn
                    }
                );
            }
        }

        private static void GetInfoColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == Level.Info) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.Green,
                        Level = Level.Info
                    }
                );
            }
        }

        private static void GetDebugColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == Level.Debug) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.Blue,
                        Level = Level.Debug
                    }
                );
            }
        }

        private static void GetTelemetryColor(
            Level level,
            List<ColoredConsoleAppender.LevelColors> map
        ) {
            if (level == TelemetryLogLevelExtension.TelemetryLevel) {
                map.Add(
                    new ColoredConsoleAppender.LevelColors {
                        ForeColor = ColoredConsoleAppender.Colors.White,
                        Level = TelemetryLogLevelExtension.TelemetryLevel
                    }
                );
            }
        }

        private static RollingFileAppender NewRollingFileAppender(
            string file,
            PatternLayout layout,
            int maxSizeRollBackups,
            string maximumFileSize
        ) {
            // Create RollingFileAppender Implementation.
            return new RollingFileAppender( ) {
                AppendToFile = true,
                PreserveLogFileNameExtension = true,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true,
                CountDirection = 1,
                File = file,
                Layout = layout,
                MaxSizeRollBackups = maxSizeRollBackups,
                MaximumFileSize = maximumFileSize
            };
        }

        private static Level[] TranslateLogLevel( SupportedLogLevels logLevels ) {
            List<Level> levels = new( );

            if (logLevels.HasFlag( SupportedLogLevels.Fatal )) { levels.Add( Level.Fatal ); }
            if (logLevels.HasFlag( SupportedLogLevels.Error )) { levels.Add( Level.Error ); }
            if (logLevels.HasFlag( SupportedLogLevels.Warn )) { levels.Add( Level.Warn ); }
            if (logLevels.HasFlag( SupportedLogLevels.Info )) { levels.Add( Level.Info ); }
            if (logLevels.HasFlag( SupportedLogLevels.Debug )) { levels.Add( Level.Debug ); }
            if (logLevels.HasFlag( SupportedLogLevels.Telemetry )) {
                levels.Add( TelemetryLogLevelExtension.TelemetryLevel );
            }

            return levels.ToArray( );
        }

        #endregion HelperMethods


        #region ILogger Additions

        public IDisposable BeginScope<TState>( TState state ) => default!;

        public bool IsEnabled( LogLevel logLevel ) {
            return logLevel switch {
                LogLevel.Critical => IsFatalEnabled( ),
                LogLevel.Error => IsErrorEnabled( ),
                LogLevel.Warning => IsWarnEnabled( ),
                LogLevel.Information => IsInfoEnabled( ),
                LogLevel.Debug => IsDebugEnabled( ),
                LogLevel.Trace => IsDebugEnabled( ),
                LogLevel.None => false,
                _ => throw new ArgumentOutOfRangeException( nameof( logLevel ) )
            };
        }

        private bool IsFatalEnabled( ) { return ILog?.IsFatalEnabled ?? false; }
        private bool IsErrorEnabled( ) { return ILog?.IsErrorEnabled ?? false; }
        private bool IsWarnEnabled( ) { return ILog?.IsWarnEnabled ?? false; }
        private bool IsInfoEnabled( ) { return ILog?.IsInfoEnabled ?? false; }
        private bool IsDebugEnabled( ) { return ILog?.IsDebugEnabled ?? false; }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) {
            if (ILog == null || IsEnabled( logLevel ) == false) { return; }

            string message = $"{formatter( state, exception )} {exception}";

            if (string.IsNullOrEmpty( message ) == false) {
                WriteFatalMessage( ILog, logLevel, message );
                WriteErrorMessage( ILog, logLevel, message );
                WriteWarnMessage( ILog, logLevel, message );
                WriteInfoMessage( ILog, logLevel, message );
                WriteDebugMessage( ILog, logLevel, message );
            }
        }

        private static void WriteFatalMessage( ILog log, LogLevel level, string message ) {
            if (level == LogLevel.Critical) { log.Fatal( message ); }
        }
        private static void WriteErrorMessage( ILog log, LogLevel level, string message ) {
            if (level == LogLevel.Error) { log.Error( message ); }
        }
        private static void WriteWarnMessage( ILog log, LogLevel level, string message ) {
            if (level == LogLevel.Warning) { log.Warn( message ); }
        }
        private static void WriteInfoMessage( ILog log, LogLevel level, string message ) {
            if (level == LogLevel.Information) { log.Info( message ); }
        }
        private static void WriteDebugMessage( ILog log, LogLevel level, string message ) {
            if (level == LogLevel.Trace || level == LogLevel.Debug) { log.Debug( message ); }
        }

        #endregion ILogger Additions
    }
}
