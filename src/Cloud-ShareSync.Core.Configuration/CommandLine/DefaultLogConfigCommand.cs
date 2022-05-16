using System.CommandLine;
using Cloud_ShareSync.Core.Configuration.ManagedActions;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Logging;

namespace Cloud_ShareSync.Core.Configuration.CommandLine {
#nullable disable

    public class DefaultLogConfigCommand : Command {

        public DefaultLogConfigCommand( Option<FileInfo> configPath ) : base(
            name: "RollingLog",
            description: "Configure the Cloud-ShareSync default rolling log settings."
        ) {

            SetFileNameOptionAlias( );
            AddOption( _fileNameOption );

            SetLogDirectoryOptionAlias( );
            AddOption( _logDirectoryOption );

            SetRolloverCountOptionAlias( );
            AddOption( _rolloverCountOption );

            SetMaximumSizeOptionAlias( );
            AddOption( _maximumSizeOption );

            SetLogLevelsOptionOptionAlias( );
            AddOption( _logLevelsOptionOption );

            AddAlias( "rollinglog" );
            AddAlias( "defaultlog" );

            SetRollingLogConfigCommandHandler( configPath );
        }

        private readonly Option<string> _fileNameOption = new(
                name: "--FileName",
                description: "Specify the file name and extension for the primary rolling log file.",
                getDefaultValue: ( ) => "Cloud-ShareSync.log"
            ) {
            IsRequired = false
        };

        private void SetFileNameOptionAlias( ) { _fileNameOption.AddAlias( "-n" ); }


        private readonly Option<DirectoryInfo> _logDirectoryOption = new(
                name: "--LogDirectory",
                description: "Specify the directory for the rolling log process to use.",
                getDefaultValue: ( ) => new( Path.Join( AppContext.BaseDirectory, "log" ) )
            ) {
            IsRequired = false
        };

        private void SetLogDirectoryOptionAlias( ) { _logDirectoryOption.AddAlias( "-d" ); }


        private readonly Option<int> _rolloverCountOption = new(
                name: "--RolloverCount",
                description: "Specify the number of MaximumSize log files to keep.",
                getDefaultValue: ( ) => 5
            ) {
            IsRequired = false
        };

        private void SetRolloverCountOptionAlias( ) { _rolloverCountOption.AddAlias( "-r" ); }


        private readonly Option<int> _maximumSizeOption = new(
                name: "--MaximumSize",
                description:
                "Specify the maximum size, in megabytes, that each log should get to before rolling over into a new file.",
                getDefaultValue: ( ) => 5
            ) {
            IsRequired = false
        };

        private void SetMaximumSizeOptionAlias( ) { _maximumSizeOption.AddAlias( "-m" ); }


        private readonly Option<SupportedLogLevels> _logLevelsOptionOption = new(
                name: "--LogLevels",
                description: "Specify the log levels that should go into the console. " +
                             "Supported Log Levels: Fatal, Error, Warn, Info, Debug, Telemetry",
                getDefaultValue: ( ) => (SupportedLogLevels)30
            ) {
            IsRequired = false
        };

        private void SetLogLevelsOptionOptionAlias( ) { _logLevelsOptionOption.AddAlias( "-l" ); }


        private void SetRollingLogConfigCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler(
                (
                    string fileName,
                    DirectoryInfo logDirectory,
                    int rolloverCount,
                    int maximumSize,
                    SupportedLogLevels logLevels,
                    FileInfo configPath
                 ) => {
                     if (configPath != null) { ConfigPathHandler.SetAltDefaultConfigPath( configPath.FullName ); }

                     DefaultLogConfig config = new( ) {
                         FileName = fileName,
                         LogDirectory = logDirectory.FullName,
                         RolloverCount = rolloverCount,
                         MaximumSize = maximumSize,
                         LogLevels = logLevels
                     };
                     new ConfigManager( ).UpdateConfigSection( config );
                 },
                _fileNameOption,
                _logDirectoryOption,
                _rolloverCountOption,
                _maximumSizeOption,
                _logLevelsOptionOption,
                configPath
            );
        }

    }
#nullable enable
}
