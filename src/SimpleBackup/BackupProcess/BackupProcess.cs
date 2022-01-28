using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.SharedServices;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static async Task BackupProcess( ) {
            using Activity? activity = s_source.StartActivity( "BackupProcess" )?.Start( );

            ConcurrentQueue<Tuple<string, BackBlazeB2Table, B2FileResponse>> validateHashMatch =
                CorrelateFilesToUploadRecords( );

            Task uploadFileLoop = UploadFileLoop( );
            Task validateFileHashLoop = ValidateUploadedFileHashMatchesLocalFileHash( validateHashMatch );

            while (validateFileHashLoop.IsCompleted == false) {
                s_logger?.ILog?.Info( "Validating file hashes for uploaded files." );
                Thread.Sleep( 5000 );
                if (uploadFileLoop.IsCompleted) { uploadFileLoop = UploadFileLoop( true ); }
            }

            await uploadFileLoop;

            activity?.Stop( );
            return;
        }

        private static async Task UploadFileLoop( bool? suppressLogMessage = null ) {
            using Activity? activity = s_source.StartActivity( "UploadFileLoop" )?.Start( );

            if (suppressLogMessage != true) {
                s_logger?.ILog?.Info( $"Begin Upload File Process. Queue contains {s_fileUploadQueue.Count} items." );
            }
            while (s_fileUploadQueue.IsEmpty == false) {
                bool deQueue = s_fileUploadQueue.TryDequeue( out string? path );

                if (deQueue && path != null) { await UploadFileProcess( path ); }
            }

            activity?.Stop( );
        }

        private static Task ValidateUploadedFileHashMatchesLocalFileHash(
            ConcurrentQueue<Tuple<string, BackBlazeB2Table, B2FileResponse>> validateHashMatch
        ) {
            using Activity? activity = s_source.StartActivity( "ValidateUploadedFileHashMatchesLocalFileHash" )?.Start( );
            s_logger?.ILog?.Info(
                $"Begin Existing Records File Hash Validation. Queue contains {validateHashMatch.Count} items."
            );

            Task[] taskArray = new Task[3];
            for (int i = 0; i < taskArray.Length; i++) {
                s_logger?.ILog?.Debug( $"Starting validation task#{i}" );
                taskArray[i] = PerformHashCheckLoop( validateHashMatch, i );
            }
            while (taskArray.Where( task => task.IsCompleted == false ).Any( )) {
                Thread.Sleep( 1000 );
            }

            activity?.Stop( );
            return Task.CompletedTask;
        }

        private static async Task PerformHashCheckLoop(
            ConcurrentQueue<Tuple<string, BackBlazeB2Table, B2FileResponse>> validateHashMatch,
            int taskNum
        ) {
            using Activity? activity = s_source.StartActivity( "PerformHashCheckLoop" )?.Start( );

            int count = 0;
            bool processed = false;
            do {
                bool deQueue = validateHashMatch.TryDequeue(
                    out Tuple<string, BackBlazeB2Table, B2FileResponse>? b2Info );
                if (deQueue && b2Info != null) {
                    s_logger?.ILog?.Debug( $"Validate File Task{taskNum}-{count}" );
                    SystemMemoryChecker.Update( );
                    string path = b2Info.Item1;
                    BackBlazeB2Table b2TableData = b2Info.Item2;
                    B2FileResponse b2File = b2Info.Item3;

                    s_logger?.ILog?.Debug( $"Validate File Task{taskNum}-{count}: Comparing local and remote hashes for '{path}'." );

                    string sha512filehash = await GetSha512FileHash( new( path ) );
                    if (
                        b2File.fileInfo.ContainsKey( "sha512_filehash" ) &&
                        b2File.fileInfo["sha512_filehash"] == sha512filehash
                    ) {
                        s_logger?.ILog?.Info(
                            $"Validate File Task{taskNum}-{count}: Local and Backblaze Sha512 hashes match for '{path}. " +
                            "Skipping upload."
                        );
                    } else {
                        s_logger?.ILog?.Info(
                            $"Validate File Task{taskNum}-{count}: Local and Backblaze Sha512 hashes DO NOT match for '{path}. " +
                            "Adding file to the upload queue."
                        );
                        s_fileUploadQueue.Enqueue( path );
                        //s_backBlaze.Delete( b2TableData.FileID );
                    }
                }

                if (validateHashMatch.IsEmpty) {
                    processed = true;
                    s_logger?.ILog?.Debug(
                        $"Validate File Task{taskNum}-{count}: validateHashMatch is Empty."
                    );
                } else {
                    count++;
                }
            } while (processed == false);
            activity?.Stop( );
        }
    }
}
