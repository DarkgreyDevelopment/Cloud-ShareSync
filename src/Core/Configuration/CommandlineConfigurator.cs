using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using Cloud_ShareSync.Backup;
using Cloud_ShareSync.Core.Configuration.Types;

namespace Cloud_ShareSync.Core.Configuration {
    internal static class CommandlineConfigurator {

        internal static void ConfigureCommandlineOptions( RootCommand rootCommand ) {
            Option<FileInfo> option = ConfigPathOption( );
            rootCommand.AddGlobalOption( option );
            rootCommand.Description = "Cloud-ShareSync";
            HandleDefaultOptions( rootCommand, option );
            AddConfigureCommand( rootCommand, option );
            AddBackupCommand( rootCommand, option );
            AddRestoreCommand( rootCommand, option );
        }

        internal static void HandleDefaultOptions(
            Command command,
            Option<FileInfo> option
        ) {
            command.SetHandler( (
                    FileInfo path,
                    InvocationContext ctx,
                    HelpBuilder helpBuilder
                ) => {
                    if (path != null) {
                        ConfigManager.SetAltDefaultConfigPath( path.FullName );
                    } else {
                        HelpContext hctx = new( ctx.HelpBuilder, command, Console.Out, null );
                        ctx.HelpBuilder.Write( hctx );
                    }
                },
                option
            );
        }

        internal static void AddConfigureCommand(
            RootCommand rootCommand,
            Option<FileInfo> option
        ) {
            Command configure = new( "Configure" );
            configure.AddAlias( "configure" );
            AddConfigureConfigPathHandler( configure, option );
            configure.Add( SyncConfig.NewSyncConfigCommand( option ) );
            configure.Add( DatabaseConfig.NewDatabaseConfigCommand( option ) );
            configure.Add( CompressionConfig.NewCompressionConfigCommand( option ) );
            configure.Add( B2Config.NewB2ConfigCommand( option ) );
            configure.Add( Log4NetConfig.NewLoggingConfigCommand( option ) );
            configure.Add( TelemetryLogConfig.NewTelemetryLogConfigCommand( option ) );
            configure.Add( DefaultLogConfig.NewRollingLogConfigCommand( option ) );
            configure.Add( ConsoleLogConfig.NewConsoleLogConfigCommand( option ) );
            rootCommand.Add( configure );
        }

        internal static void AddConfigureConfigPathHandler(
            Command configure,
            Option<FileInfo> option
        ) {
            configure.SetHandler( (
                    FileInfo path,
                    InvocationContext ctx,
                    HelpBuilder helpBuilder
                ) => {
                    if (path != null) {
                        ConfigManager.SetAltDefaultConfigPath( path.FullName );

                        CompleteConfig defaultConfig = new( new( "{SyncFolder}" ) );
                        Console.WriteLine( $"Writing default Cloud-ShareSync config to '{path.FullName}'." );
                        File.WriteAllText( path.FullName, defaultConfig.ToString( ) );
                    } else {
                        HelpContext hctx = new( ctx.HelpBuilder, configure, Console.Out, null );
                        ctx.HelpBuilder.Write( hctx );
                    }
                },
                option
            );
        }

        internal static void AddBackupCommand( RootCommand rootCommand, Option<FileInfo> option ) {
            Command backup = new( "Backup" );
            backup.AddAlias( "backup" );
            backup.SetHandler(
                ( FileInfo path ) => {
                    if (path != null) { ConfigManager.SetAltDefaultConfigPath( path.FullName ); }

                    Process backup = new( );
                    backup.Run( ).GetAwaiter( ).GetResult( );
                },
                option
            );
            rootCommand.Add( backup );
        }

        internal static void AddRestoreCommand( RootCommand rootCommand, Option<FileInfo> option ) {
            Command restore = new( "Restore" );
            restore.AddAlias( "restore" );
            restore.SetHandler(
                ( FileInfo path ) => {
                    if (path != null) { ConfigManager.SetAltDefaultConfigPath( path.FullName ); }
                    Console.WriteLine( "Restore: " );
                },
                option
            );
            rootCommand.Add( restore );
        }

        internal static Option<FileInfo> ConfigPathOption( ) {
            Option<FileInfo> configPath = new( "--ConfigPath", "The path to the applications appsettings.json file." );
            configPath.AddAlias( "--configpath" );
            configPath.AddAlias( "-path" );
            return configPath;
        }

    }
}
