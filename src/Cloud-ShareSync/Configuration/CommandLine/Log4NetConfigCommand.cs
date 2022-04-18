using System.CommandLine;
using Cloud_ShareSync.Configuration.ManagedActions;
using Cloud_ShareSync.Configuration.Types;

namespace Cloud_ShareSync.Configuration.CommandLine {

    public class Log4NetConfigCommand : Command {

        public Log4NetConfigCommand( Option<FileInfo> configPath ) : base(
            name: "Logging",
            description: "Configure the Cloud-ShareSync logging settings."
        ) {
            SetConfigurationFileOptionAlias( );
            AddOption( _configurationFileOption );

            SetEnableDefaultLogOptionAlias( );
            AddOption( _enableDefaultLogOption );

            SetEnableTelemetryLogOptionAlias( );
            AddOption( _enableTelemetryLogOption );

            SetEnableConsoleLogOptionAlias( );
            AddOption( _enableConsoleLogOption );

            AddAlias( "logging" );
            AddAlias( "log" );

            SetLoggingConfigCommandHandler( configPath );
        }


        private readonly Option<string> _configurationFileOption = new(
                name: "--ConfigurationFile",
                description: "Specify the path to a log4net (xml) config file.",
                getDefaultValue: ( ) => string.Empty
            ) {
            IsRequired = false
        };

        private void SetConfigurationFileOptionAlias( ) { _configurationFileOption.AddAlias( "-cf" ); }


        private readonly Option<bool> _enableDefaultLogOption = new(
                name: "--EnableDefaultLog",
                description: "Enable/disable the default rolling log file process.",
                getDefaultValue: ( ) => true
            ) {
            IsRequired = false
        };

        private void SetEnableDefaultLogOptionAlias( ) { _enableDefaultLogOption.AddAlias( "-d" ); }


        private readonly Option<bool> _enableTelemetryLogOption = new(
                name: "--EnableTelemetryLog",
                description: "Enable/disable the telemetry log file process.",
                getDefaultValue: ( ) => false
            ) {
            IsRequired = false
        };

        private void SetEnableTelemetryLogOptionAlias( ) { _enableTelemetryLogOption.AddAlias( "-t" ); }


        private readonly Option<bool> _enableConsoleLogOption = new(
                name: "--EnableConsoleLog",
                description: "Enable/disable the console log process.",
                getDefaultValue: ( ) => true
            ) {
            IsRequired = false
        };

        private void SetEnableConsoleLogOptionAlias( ) { _enableConsoleLogOption.AddAlias( "-c" ); }

        private void SetLoggingConfigCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler(
                (
                    string configurationFile,
                    bool enableDefaultLog,
                    bool enableTelemetryLog,
                    bool enableConsoleLog,
                    FileInfo configPath
                ) => {
                    if (configPath != null) { ConfigPathHandler.SetAltDefaultConfigPath( configPath.FullName ); }

                    Log4NetConfig config = new( ) {
                        ConfigurationFile = configurationFile,
                        EnableDefaultLog = enableDefaultLog,
                        EnableTelemetryLog = enableTelemetryLog,
                        EnableConsoleLog = enableConsoleLog
                    };
                    new ConfigManager( ).UpdateConfigSection( config );
                },
                _configurationFileOption,
                _enableDefaultLogOption,
                _enableTelemetryLogOption,
                _enableConsoleLogOption,
                configPath
            );
        }

    }
}
