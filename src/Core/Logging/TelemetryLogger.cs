using System.Reflection;
using System.Text;
using Cloud_ShareSync.Core.Configuration.Enums;
using Cloud_ShareSync.Core.Configuration.Types;
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

        public static readonly Level[] ErrLvl = new Level[] { Level.Fatal, Level.Error };

        #region Ctor

        public TelemetryLogger( Log4NetConfig? config = null ) {
            _loggerRepository = LogManager.GetRepository( Assembly.GetEntryAssembly( ) );
            if (config != null) {
                try {
                    ConfigureLog4NetFromConfig( config );
                    ILog = LogManager.GetLogger( ServiceName );
                } catch (Exception ex) {
                    Console.Error.WriteLine(
                        "\nAn error has occurred - Failed to configure Application Logger. " +
                        "The application will have exceptionally limited log messages.\n" +
                        $"Error: {ex.Message}\n" +
                        $"{ex}\n"
                    );
                }
            }
            OpenTelemetry = ConfigureOpenTelemetryTracer( );
        }

        private void ConfigureLog4NetFromConfig( Log4NetConfig config ) {
            if (string.IsNullOrWhiteSpace( config.ConfigurationFile )) {
                Hierarchy hierarchy = (Hierarchy)_loggerRepository;
                hierarchy.LevelMap.Add( TelemetryLogLevelExtension.TelemetryLevel );
                if (config.EnableDefaultLog) { ConfigureDefaultLogAppender( hierarchy, config.DefaultLogConfiguration ); }
                if (config.EnableTelemetryLog) { ConfigureTelemetryAppender( hierarchy, config.TelemetryLogConfiguration ); }
                if (config.EnableConsoleLog) { ConfigureConsoleAppender( hierarchy, config.ConsoleConfiguration ); }
                hierarchy.Configured = true;
            } else /* log4NetConfig.ConfigurationFile has content */ {
                ConfigureFromLog4NetXmlConfigFile( config );
            }
        }

        private void ConfigureFromLog4NetXmlConfigFile( Log4NetConfig config ) {
            if (File.Exists( config.ConfigurationFile ) == false) {
                throw new FileNotFoundException(
                    $"Log4Net ConfigurationFile '{config.ConfigurationFile}' doesn't exist."
                );
            }

            _ = XmlConfigurator.Configure(
                _loggerRepository,
                new FileInfo( config.ConfigurationFile )
            );
        }

        private TracerProvider ConfigureOpenTelemetryTracer( ) {
            string[] sources = new string[] {
                "B2", "BackBlazeB2.PublicInterface", "Cloud-ShareSync.Backup.Program",
                "CompressionInterface", "ConfigManager", "Hashing", "HostProvider",
                "ManagedChaCha20Poly1305", "MimeType", "PrepUploadFileProcess",
                "UniquePassword", "UploadFileProcess"
            };
            return Sdk.CreateTracerProviderBuilder( )
                .AddSource( sources )
                .SetResourceBuilder( ResourceBuilder.CreateDefault( )
                .AddService( ServiceName ) )
                .AddProcessor( new SimpleActivityExportProcessor( new LogExporter( ILog ) ) )
                .AddLogExporter( ILog )
                .Build( );
        }

        #endregion Ctor


        #region CreateAppenders

        private static void ConfigureDefaultLogAppender(
            Hierarchy hierarchy,
            DefaultLogConfig? logConfig
        ) {

            if (logConfig == null) {
                Console.Error.WriteLine(
                    "Failed to add default Log4Net RollingLogAppender. Logging may be limited.\n" +
                    "Cannot Enable Default Log if Log Config Is Null."
                );
                return;
            }

            PatternLayout patternLayout = NewPatternLayout( LogMessageFormat );

            // Create RollingFileAppender Implementation
            RollingFileAppender roller = NewRollingFileAppender(
                Path.Join( logConfig.LogDirectory, logConfig.FileName ),
                patternLayout,
                logConfig.RolloverCount,
                logConfig.MaximumSize + "MB"
            );

            Level[] levels = TranslateLogLevel( logConfig.LogLevels );

            AddFilters( roller, CreateFiltersList( levels, false ) );

            roller.ActivateOptions( );
            hierarchy.Root.AddAppender( roller );
        }

        private static void ConfigureTelemetryAppender(
            Hierarchy hierarchy,
            TelemetryLogConfig? telemetryConfig
        ) {
            if (telemetryConfig == null) {
                Console.Error.WriteLine(
                    "Failed to add telemetry Log4Net RollingLogAppender.\n" +
                    "Cannot Enable Telemetry Log if TelemetryConfig Is Null."
                );
                return;
            }

            PatternLayout patternLayout = NewPatternLayout( "%m%n" );

            // Create RollingFileAppender Implementation.
            RollingFileAppender telemetry = NewRollingFileAppender(
                Path.Join( telemetryConfig.LogDirectory, telemetryConfig.FileName ),
                patternLayout,
                telemetryConfig.RolloverCount,
                telemetryConfig.MaximumSize + "MB"
            );

            Level[] telemetryLevel = new Level[] { TelemetryLogLevelExtension.TelemetryLevel };

            AddFilters( telemetry, CreateFiltersList( telemetryLevel, false ) );

            telemetry.ActivateOptions( );
            hierarchy.Root.AddAppender( telemetry );
        }

        private static void ConfigureConsoleAppender(
            Hierarchy hierarchy,
            ConsoleLogConfig? consoleConfig
        ) {
            if (consoleConfig == null) {
                Console.Error.WriteLine(
                    "Failed to add Log4Net ConsoleAppender. Console logging will be limited." +
                    "Cannot Enable Console Log if ConsoleConfig Is Null."
                );
                return;
            }

            string? envNoColor = Environment.GetEnvironmentVariable( "NO_COLOR" );

            if (consoleConfig.EnableColoredConsole && envNoColor == null) {
                ConfigureColoredConsoleAppender( hierarchy, consoleConfig );
            } else {
                ConfigureRegularConsoleAppender( hierarchy, consoleConfig );
            }
        }

        private static void ConfigureColoredConsoleAppender(
            Hierarchy hierarchy,
            ConsoleLogConfig consoleConfig
        ) {
            try {
                RegisterCodePage( );
                PatternLayout patternLayout = NewPatternLayout( LogMessageFormat );
                ColoredConsoleAppender consoleAppender = NewColoredConsoleAppender( patternLayout );
                ConfigureErrorColoredConsoleAppender( consoleConfig.UseStdErr, hierarchy, patternLayout );
                Level[] levels = TranslateLogLevel( consoleConfig.LogLevels, consoleConfig.UseStdErr );
                AddFilters( consoleAppender, CreateFiltersList( levels, consoleConfig.UseStdErr ) );
                AddMappings( consoleAppender, CreateMappingsList( levels ) );
                consoleAppender.ActivateOptions( );
                hierarchy.Root.AddAppender( consoleAppender );
            } catch (Exception consoleLogFailure) {
                Console.Error.WriteLine(
                    "Failed to add Log4Net ColoredConsoleAppender. Console logging will be limited.\n" +
                    consoleLogFailure.ToString( )
                );
            }
        }

        private static void ConfigureErrorColoredConsoleAppender(
            bool useStdErr,
            Hierarchy hierarchy,
            PatternLayout pattern
        ) {
            if (useStdErr == false) { return; }
            ColoredConsoleAppender consoleErrorAppender = NewColoredConsoleAppender( pattern, true );
            AddErrorFilters( consoleErrorAppender );
            AddMappings( consoleErrorAppender, CreateMappingsList( ErrLvl ) );
            consoleErrorAppender.ActivateOptions( );
            hierarchy.Root.AddAppender( consoleErrorAppender );
        }

        private static void ConfigureRegularConsoleAppender(
            Hierarchy hierarchy,
            ConsoleLogConfig consoleConfig
        ) {
            try {
                RegisterCodePage( );
                PatternLayout patternLayout = NewPatternLayout( LogMessageFormat );
                ConsoleAppender consoleAppender = NewConsoleAppender( patternLayout );
                ConfigureErrorConsoleAppender( consoleConfig.UseStdErr, hierarchy, patternLayout );
                Level[] levels = TranslateLogLevel( consoleConfig.LogLevels, consoleConfig.UseStdErr );
                AddFilters( consoleAppender, CreateFiltersList( levels, consoleConfig.UseStdErr ) );
                consoleAppender.ActivateOptions( );
                hierarchy.Root.AddAppender( consoleAppender );
            } catch (Exception consoleLogFailure) {
                Console.Error.WriteLine(
                    "Failed to add Log4Net ConsoleAppender. Console logging will be limited.\n" +
                    consoleLogFailure.ToString( )
                );
            }
        }

        private static void ConfigureErrorConsoleAppender(
            bool useStdErr,
            Hierarchy hierarchy,
            PatternLayout pattern
        ) {
            if (useStdErr == false) { return; }
            ConsoleAppender consoleErrorAppender = NewConsoleAppender( pattern, true );
            AddErrorFilters( consoleErrorAppender );
            consoleErrorAppender.ActivateOptions( );
            hierarchy.Root.AddAppender( consoleErrorAppender );
        }

        #endregion CreateAppenders


        #region HelperMethods

        private static void RegisterCodePage( ) {
            try {
                // Register terminal code page for console output.
                Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );
            } catch (Exception ex) {
                Console.Error.WriteLine(
                    "Failed to register console codepage. Console logging may be limited. Error:\n" +
                    ex.ToString( )
                );
            }
        }

        private static ConsoleAppender NewConsoleAppender( PatternLayout pattern, bool targetError = false ) {
            ConsoleAppender consoleAppender = new( ) { Layout = pattern };
            if (targetError) { consoleAppender.Target = "Console.Error"; }
            return consoleAppender;
        }

        private static ColoredConsoleAppender NewColoredConsoleAppender( PatternLayout pattern, bool targetError = false ) {
            ColoredConsoleAppender consoleAppender = new( ) { Layout = pattern };
            if (targetError) { consoleAppender.Target = "Console.Error"; }
            return consoleAppender;
        }

        private static PatternLayout NewPatternLayout( string logMessageFormat ) {
            PatternLayout patternLayout = new( ) { ConversionPattern = logMessageFormat };
            patternLayout.ActivateOptions( );
            return patternLayout;
        }

        private static void AddFilters( AppenderSkeleton appender, List<log4net.Filter.IFilter> list ) {
            foreach (log4net.Filter.IFilter filter in list) { appender.AddFilter( filter ); }
        }

        private static void AddErrorFilters( AppenderSkeleton consoleErrorAppender ) {
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
            consoleErrorAppender.AddFilter( new log4net.Filter.DenyAllFilter( ) );
        }

        private static void AddMappings(
            ColoredConsoleAppender appender,
            List<ColoredConsoleAppender.LevelColors> mappings
        ) {
            foreach (ColoredConsoleAppender.LevelColors mapping in mappings) {
                appender.AddMapping( mapping );
            }
        }

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

        private static Level[] TranslateLogLevel( SupportedLogLevels logLevels, bool? excludeErrors = null ) {
            List<Level> levels = new( );
            AddFatalLevel( logLevels, levels, excludeErrors );
            AddErrorLevel( logLevels, levels, excludeErrors );
            AddWarnLevel( logLevels, levels );
            AddInfoLevel( logLevels, levels );
            AddDebugLevel( logLevels, levels );
            AddTelemetryLevel( logLevels, levels );
            return levels.ToArray( );
        }

        private static void AddFatalLevel( SupportedLogLevels logLevels, List<Level> levels, bool? excludeErrors ) {
            if (logLevels.HasFlag( SupportedLogLevels.Fatal ) && excludeErrors == true) { levels.Add( Level.Fatal ); }
        }

        private static void AddErrorLevel( SupportedLogLevels logLevels, List<Level> levels, bool? excludeErrors ) {
            if (logLevels.HasFlag( SupportedLogLevels.Error ) && excludeErrors == true) { levels.Add( Level.Error ); }
        }

        private static void AddWarnLevel( SupportedLogLevels logLevels, List<Level> levels ) {
            if (logLevels.HasFlag( SupportedLogLevels.Warn )) { levels.Add( Level.Warn ); }
        }

        private static void AddInfoLevel( SupportedLogLevels logLevels, List<Level> levels ) {
            if (logLevels.HasFlag( SupportedLogLevels.Info )) { levels.Add( Level.Info ); }
        }

        private static void AddDebugLevel( SupportedLogLevels logLevels, List<Level> levels ) {
            if (logLevels.HasFlag( SupportedLogLevels.Debug )) { levels.Add( Level.Debug ); }
        }

        private static void AddTelemetryLevel( SupportedLogLevels logLevels, List<Level> levels ) {
            if (logLevels.HasFlag( SupportedLogLevels.Telemetry )) {
                levels.Add( TelemetryLogLevelExtension.TelemetryLevel );
            }
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
