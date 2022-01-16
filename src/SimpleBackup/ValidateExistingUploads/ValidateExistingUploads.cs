﻿using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void ValidateExistingUploads( ) {
            using Activity? activity = s_source.StartActivity( "ValidateExistingUploads" )?.Start( );
            s_logger?.ILog?.Debug( "Start ValidateExistingUploads" );

            List<Tuple<BackBlazeB2Table, B2FileResponse>> correlatedRecords =
                CorrelateBackBlazeRecords( BackBlazeB2.ListFileVersions( ).Result );

            if (s_fileUploadQueue.IsEmpty == false && correlatedRecords.Count > 0) {
                ConcurrentQueue<string> fileUploadQueue = SnapshotConcurrentQueue( s_fileUploadQueue );

                bool validated = false;
                do {
                    bool deQueue = fileUploadQueue.TryDequeue( out string? path );
                    if (deQueue && path != null) {

                        bool shouldQueue = true;

                        PrimaryTable? tabledata = TryGetTableDataForUpload( path );

                        Tuple<BackBlazeB2Table, B2FileResponse>? b2info = correlatedRecords
                                                            .Where( x => x.Item1.Id == tabledata?.Id )
                                                            .FirstOrDefault( );
                        if (b2info != null) {
                            BackBlazeB2Table b2TableData = b2info.Item1;
                            B2FileResponse b2File = b2info.Item2;

                            string sha512filehash = GetSha512FileHash( new( path ) ).Result;
                            if (
                                b2File.fileInfo.ContainsKey( "sha512_filehash" ) &&
                                b2File.fileInfo["sha512_filehash"] == sha512filehash
                            ) {
                                s_logger?.ILog?.Info(
                                    $"Local and Backblaze Sha512 hashes match for '{path}. Skipping upload."
                                );
                                shouldQueue = false;
                            }
                        }

                        if (shouldQueue) {
                            s_logger?.ILog?.Debug( $"Path '{path}' should be uploaded to backblaze." );
                            s_fileUploadQueue.Enqueue( path );
                        }
                    }

                    if (fileUploadQueue.IsEmpty) { validated = true; }
                } while (validated == false);
            }

            s_logger?.ILog?.Debug( "Finished ValidateExistingUploads" );

            activity?.Stop( );
        }

    }
}
