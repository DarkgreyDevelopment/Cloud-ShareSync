﻿using System.Collections.Concurrent;
using System.Diagnostics;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static ConcurrentQueue<Tuple<string, BackBlazeB2Table, B2FileResponse>> CorrelateFilesToUploadRecords( ) {
            using Activity? activity = s_source.StartActivity( "CorrelateFilesToUploadRecords" )?.Start( );
            s_logger?.ILog?.Debug( "Begin correlating files to upload records" );

            if (s_backBlaze == null || s_config?.SimpleBackup == null) {
                throw new InvalidOperationException( "Cannot proceed if backblaze configuration is not initialized." );
            }

            ConcurrentQueue<Tuple<string, BackBlazeB2Table, B2FileResponse>> result = new( );

            List<Tuple<BackBlazeB2Table, B2FileResponse>> correlatedRecords =
                CorrelateBackBlazeRecords( s_backBlaze.ListFileVersions( ).Result );

            // Query Primary Table to correlate backblaze records w/ primary records.
            long[] ids = correlatedRecords.Select( a => a.Item1.Id ).ToArray( );
            PrimaryTable[] primaryTableData = TryGetTableDataForUpload( ids );

            if (s_fileUploadQueue.IsEmpty == false && correlatedRecords.Count > 0) {
                ConcurrentQueue<string> fileUploadQueue = SnapshotConcurrentQueue( s_fileUploadQueue );

                bool processed = false;

                do {
                    bool deQueue = fileUploadQueue.TryDequeue( out string? path );
                    if (deQueue && path != null) {
                        // Find appropriate primary table record.
                        PrimaryTable? tabledata = primaryTableData.Where( e =>
                            e.FileName == new FileInfo( path ).Name &&
                            e.UploadPath == Path.GetRelativePath( s_config.SimpleBackup.RootFolder, path )
                        ).FirstOrDefault( );

                        // Correlate to BackBlaze
                        Tuple<BackBlazeB2Table, B2FileResponse>? b2info = correlatedRecords
                                    .Where( x => x.Item1.Id == tabledata?.Id )
                                    .FirstOrDefault( );

                        if (b2info == null) {
                            s_fileUploadQueue.Enqueue( path );
                        } else {
                            result.Enqueue( new( path, b2info.Item1, b2info.Item2 ) );
                        }
                    }

                    if (fileUploadQueue.IsEmpty) { processed = true; }
                } while (processed == false);

            }
            s_logger?.ILog?.Debug( $"Finished correlating files to upload records. Found {result.Count} existing records." );

            activity?.Stop( );
            return result;
        }

    }
}
