using System.CommandLine;
using Cloud_ShareSync.Core.Configuration.ManagedActions;
using Cloud_ShareSync.Core.Configuration.Types;

namespace Cloud_ShareSync.Core.Configuration.CommandLine {
#nullable disable

    public class TelemetryLogConfigCommand : Command {

        public TelemetryLogConfigCommand( Option<FileInfo> configPath ) : base(
            name: "TelemetryLog",
            description: "Configure the Cloud-ShareSync telemetry rolling log settings."
        ) {
            SetFileNameOptionAlias( );
            AddOption( _fileNameOption );

            SetLogDirectoryOptionAlias( );
            AddOption( _logDirectoryOption );

            SetRolloverCountOptionAlias( );
            AddOption( _rolloverCountOption );

            SetMaximumSizeOptionAlias( );
            AddOption( _maximumSizeOption );

            AddAlias( "telemetrylog" );

            SetTelemetryLogConfigCommandHandler( configPath );
        }


        private readonly Option<string> _fileNameOption = new(
                name: "--FileName",
                description: "Specify the file name and extension for the primary telemetry log file.",
                getDefaultValue: ( ) => "Cloud-ShareSync.log"
            ) {
            IsRequired = false
        };

        private void SetFileNameOptionAlias( ) { _fileNameOption.AddAlias( "-n" ); }


        private readonly Option<DirectoryInfo> _logDirectoryOption = new(
                name: "--LogDirectory",
                description: "Specify the directory for the telemetry log process to use.",
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


        private void SetTelemetryLogConfigCommandHandler( Option<FileInfo> configPath ) {
            this.SetHandler(
                (
                     string fileName,
                     DirectoryInfo logDirectory,
                     int rolloverCount,
                     int maximumSize,
                     FileInfo configPath
                 ) => {
                     if (configPath != null) { ConfigPathHandler.SetAltDefaultConfigPath( configPath.FullName ); }

                     TelemetryLogConfig config = new( ) {
                         FileName = fileName,
                         LogDirectory = logDirectory.FullName,
                         RolloverCount = rolloverCount,
                         MaximumSize = maximumSize

                     };
                     new ConfigManager( ).UpdateConfigSection( config );
                 },
                _fileNameOption,
                _logDirectoryOption,
                _rolloverCountOption,
                _maximumSizeOption,
                configPath
            );
        }

    }
#nullable enable
}
