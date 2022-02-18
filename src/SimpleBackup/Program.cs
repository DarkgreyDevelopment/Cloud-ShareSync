﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.Configuration;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.SharedServices;
using Cloud_ShareSync.Core.SharedServices.BackgroundService;
using Cloud_ShareSync.Core.SharedServices.BackgroundService.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.SimpleBackup {

    public class Program {
        private static readonly ActivitySource s_source = new( "Cloud_ShareSync.SimpleBackup.Program" );

        internal static async Task Main( string[] args ) {
            ILogger? log = null;
            try {
                CompleteConfig config = ConfigManager.GetConfiguration( args );
                log = ConfigManager.ConfigureTelemetryLogger( config.Log4Net, Array.Empty<string>( ) );
                using Activity? activity = s_source.StartActivity( "Main" )?.Start( );
                ConfigManager.ValidateConfigSet( config, false, true, log );
                SystemMemoryChecker.Inititalize( log );
                if (config?.Backup == null) {
                    throw new InvalidOperationException( "Cannot continue if SimpleBackup Config is null." );
                }
                List<string> fileList = PopulateFileList(
                    config.Backup,
                    log
                );

                IHost host = HostProvider.ConfigureHost( log, args, config );

                await RunSimpleBackupProcess( host, fileList, log );

                log?.LogInformation( "Simple backup completed." );
                activity?.Stop( );
            } catch (Exception e) {
                if (log == null) {
                    Console.WriteLine( e.ToString( ) );
                } else {
                    log.LogCritical( "{exception}", e );
                }
            }
        }

        private static List<string> PopulateFileList(
            BackupConfig config,
            ILogger? log = null
        ) {
            using Activity? activity = s_source.StartActivity( "PopulateFileList" )?.Start( );

            Regex[] excludePatterns = BuildExcludeRegexArray( config.ExcludePaths );

            IEnumerable<string> files = EnumerateRootFolder( config, log );
            List<string> fileList = new( );

            log?.LogInformation( "Building file upload queue." );
            int count = 0;
            foreach (string path in files) {
                bool includePath = true;
                foreach (Regex pattern in excludePatterns) {
                    if (pattern.Match( path ).Success) {
                        includePath = false;
                        break;
                    }
                }

                if (includePath && fileList.Contains( path ) == false) {
                    fileList.Add( path );
                } else {
                    log?.LogDebug( "Skipping excluded file: '{string}'", path );
                }
                count++;
            }

            log?.LogInformation( "File upload queue contains {int} files.", fileList.Count );
            activity?.Stop( );
            return fileList;
        }

        private static Regex[] BuildExcludeRegexArray( string[]? excludePaths ) {
            using Activity? activity = s_source.StartActivity( "BuildExcludeRegexArray" )?.Start( );

            List<Regex> regexPatterns = new( );
            if (excludePaths != null) {
                foreach (string exPath in excludePaths) {
                    regexPatterns.Add( new( exPath, RegexOptions.Compiled ) );
                }
            }

            activity?.Stop( );
            return regexPatterns.ToArray( );
        }

        private static IEnumerable<string> EnumerateRootFolder(
            BackupConfig config,
            ILogger? log = null
        ) {
            using Activity? activity = s_source.StartActivity( "EnumerateRootFolder" )?.Start( );

            string txt = config.MonitorSubDirectories ? " recursively " : " ";
            log?.LogInformation( "Populating file list{string}from root folder '{string}'.", txt, config.RootFolder );

            IEnumerable<string> files = config.RootFolder == null ?
                Enumerable.Empty<string>( ) :
                Directory.EnumerateFiles(
                    config.RootFolder,
                    "*",
                    config.MonitorSubDirectories ?
                        SearchOption.AllDirectories :
                        SearchOption.TopDirectoryOnly
                );
            log?.LogInformation( "Discovered {int} files under '{string}'.", files.Count( ), config.RootFolder );

            activity?.Stop( );
            return files;
        }

        private static async Task RunSimpleBackupProcess( IHost host, List<string> fileList, ILogger? log ) {

            await PrepWork( host, fileList, log );

            Task[] prepTasks = new Task[5];
            for (int i = 0; i < prepTasks.Length; i++) {
                prepTasks[i] = PrepProcess( host, log );
            }

            Task[] uploadTasks = new Task[2];
            for (int i = 0; i < uploadTasks.Length; i++) {
                uploadTasks[i] = UploadWork( host, log );
            }

            while (prepTasks.Any( e => e.IsCompleted != true ) || uploadTasks.Any( e => e.IsCompleted != true )) {
                Thread.Sleep( 1000 );
                if (uploadTasks.All( e => e.IsCompleted == true ) && IUploadFileProcess.Queue.IsEmpty == false) {
                    for (int i = 0; i < uploadTasks.Length; i++) {
                        uploadTasks[i] = UploadWork( host, log );
                    }
                }
            }
        }

        private static async Task PrepWork( IHost host, List<string> fileList, ILogger? log ) {
            log?.LogDebug( "Begin Prep Work." );
            IPrepUploadFileProcess prepWorker = host.Services.GetRequiredService<IPrepUploadFileProcess>( );
            await prepWorker.Prep( fileList );
            log?.LogDebug( "Completed Prep Work." );
        }

        private static async Task PrepProcess( IHost host, ILogger? log ) {
            log?.LogDebug( "Begin Prep Process." );
            IPrepUploadFileProcess prepWorker = host.Services.GetRequiredService<IPrepUploadFileProcess>( );
            await prepWorker.Process( );
            log?.LogDebug( "Completed Prep Process." );
        }

        private static async Task UploadWork( IHost host, ILogger? log ) {
            log?.LogDebug( "Begin Upload Work." );
            IUploadFileProcess uploadWorker = host.Services.GetRequiredService<IUploadFileProcess>( );
            await uploadWorker.Process( );
            log?.LogDebug( "Completed Upload Work." );
        }

    }
}
