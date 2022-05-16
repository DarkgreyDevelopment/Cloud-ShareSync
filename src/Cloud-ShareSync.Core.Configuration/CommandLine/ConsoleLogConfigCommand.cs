using System.CommandLine;
using Cloud_ShareSync.Core.Configuration.ManagedActions;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Logging;

namespace Cloud_ShareSync.Core.Configuration.CommandLine {
#nullable disable
    public class ConsoleLogConfigCommand : Command {

        public ConsoleLogConfigCommand( Option<FileInfo> configPath ) : base(
            name: "ConsoleLog",
            description: "Configure the Cloud-ShareSync console log settings."
        ) {
            SetUseStdErrOptionAlias( );
            AddOption( _useStdErrOption );

            SetEnableColoredConsoleOptionAlias( );
            AddOption( _enableColoredConsoleOption );

            SetLogLevelsOptionOptionAlias( );
            AddOption( _logLevelsOptionOption );

            AddAlias( "consolelog" );

            SetConsoleLogConfigCommandHandler( configPath );
        }


        private readonly Option<bool> _useStdErrOption = new(
                name: "--UseStdErr",
                description:
                    "Enable to send error messages to the stderr stream. Disable to send all output to the stdout stream.",
                getDefaultValue: ( ) => true
            ) {
            IsRequired = false
        };

        private void SetUseStdErrOptionAlias( ) { _useStdErrOption.AddAlias( "-e" ); }


        private readonly Option<bool> _enableColoredConsoleOption = new(
                name: "--EnableColoredConsole",
                description:
                "Enable to use colored console messages. Disable or set $Env:NO_COLOR to true to disable colored messages.",
                getDefaultValue: ( ) => true
            ) {
            IsRequired = false
        };

        private void SetEnableColoredConsoleOptionAlias( ) { _enableColoredConsoleOption.AddAlias( "-c" ); }


        private readonly Option<SupportedLogLevels> _logLevelsOptionOption = new(
                name: "--LogLevels",
                description: "Specify the log levels that should go into the console. " +
                             "Supported Log Levels: Fatal, Error, Warn, Info, Debug, Telemetry",
                getDefaultValue: ( ) => (SupportedLogLevels)30
            ) {
            IsRequired = false
        };

        private void SetLogLevelsOptionOptionAlias( ) { _logLevelsOptionOption.AddAlias( "-l" ); }


        private void SetConsoleLogConfigCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler( (
                     bool useStdErr,
                     bool enableColoredConsole,
                     SupportedLogLevels logLevels,
                     FileInfo configPath
                 ) => {
                     if (configPath != null) { ConfigPathHandler.SetAltDefaultConfigPath( configPath.FullName ); }

                     ConsoleLogConfig config = new( ) {
                         UseStdErr = useStdErr,
                         EnableColoredConsole = enableColoredConsole,
                         LogLevels = logLevels
                     };
                     new ConfigManager( ).UpdateConfigSection( config );
                 },
                _useStdErrOption,
                _enableColoredConsoleOption,
                _logLevelsOptionOption,
                configPath
            );
        }

    }
#nullable enable
}
