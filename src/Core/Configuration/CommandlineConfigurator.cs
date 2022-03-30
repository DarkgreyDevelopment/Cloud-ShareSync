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


        internal static Option<FileInfo> ConfigPathOption( ) {
            Option<FileInfo> configPath = new( "--ConfigPath", "The path to the applications appsettings.json file." );
            configPath.AddAlias( "--configpath" );
            configPath.AddAlias( "-path" );
            return configPath;
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
            Option<FileInfo> configPath
        ) {
            Command configure = new( "Configure" );
            configure.AddAlias( "configure" );
            AddConfigureHandler( configure, configPath );
            configure.Add( SyncConfig.NewSyncConfigCommand( configPath ) );
            configure.Add( DatabaseConfig.NewDatabaseConfigCommand( configPath ) );
            configure.Add( CompressionConfig.NewCompressionConfigCommand( configPath ) );
            configure.Add( B2Config.NewB2ConfigCommand( configPath ) );
            configure.Add( Log4NetConfig.NewLoggingConfigCommand( configPath ) );
            configure.Add( TelemetryLogConfig.NewTelemetryLogConfigCommand( configPath ) );
            configure.Add( DefaultLogConfig.NewRollingLogConfigCommand( configPath ) );
            configure.Add( ConsoleLogConfig.NewConsoleLogConfigCommand( configPath ) );
            rootCommand.Add( configure );
        }

        internal static void AddConfigureHandler(
            Command configure,
            Option<FileInfo> option
        ) {
            Option<bool> createConfig = CreateConfigOption( );
            configure.Add( createConfig );

            configure.SetHandler( (
                    FileInfo path,
                    bool create,
                    InvocationContext ctx,
                    HelpBuilder helpBuilder
                ) => {
                    if (path != null) {
                        ConfigManager.SetAltDefaultConfigPath( path.FullName );
                        if (create) {
                            CompleteConfig defaultConfig = new( new( SyncConfig.DefaultSyncFolder ) );
                            Console.WriteLine( $"Writing default Cloud-ShareSync config to '{path.FullName}'." );
                            File.WriteAllText( path.FullName, defaultConfig.ToString( ) );
                        }
                    } else {
                        HelpContext hctx = new( ctx.HelpBuilder, configure, Console.Out, null );
                        ctx.HelpBuilder.Write( hctx );
                    }
                },
                option,
                createConfig
            );
        }

        internal static Option<bool> CreateConfigOption( ) {
            Option<bool> createConfig = new(
                name: "--CreateConfig",
                description: "Use with --ConfigPath to create a new default Cloud-ShareSync configuration file.",
                getDefaultValue: ( ) => false
            );
            createConfig.AddAlias( "--createconfig" );
            createConfig.AddAlias( "-create" );
            return createConfig;
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

    }
}
