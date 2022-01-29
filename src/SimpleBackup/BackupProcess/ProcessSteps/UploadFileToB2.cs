﻿using System.Diagnostics;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Configuration.Types.Cloud;
using Cloud_ShareSync.Core.Database.Entities;
using Cloud_ShareSync.Core.Database.Sqlite;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static async Task UploadFileToB2(
            FileInfo uploadFile,
            string fileName,
            string uploadPath,
            string sha512Hash,
            PrimaryTable tabledata,
            B2Config config,
            BackupConfig backupConfig
        ) {
            using Activity? activity = s_source.StartActivity( "UploadFileToB2" )?.Start( );

            if (s_backBlaze == null) {
                throw new InvalidOperationException( "Cannot proceed if backblaze configuration is not initialized." );
            }

            if (backupConfig.ObfuscateUploadedFileNames) {
                uploadPath = sha512Hash;
            }

            s_logger?.ILog?.Info( "Uploading File To BackBlaze." );
            string fileId = await s_backBlaze.UploadFile(
                        uploadFile,
                        fileName,
                        uploadPath,
                        sha512Hash
                    );

            SqliteContext sqliteContext = GetSqliteContext( );
            BackBlazeB2Table? b2TableData = TryGetBackBlazeB2Data( tabledata.Id, sqliteContext );

            if (b2TableData == null) {
                sqliteContext.Add(
                    new BackBlazeB2Table(
                        tabledata.Id,
                        config.BucketName,
                        config.BucketId,
                        fileId
                    ) );
                sqliteContext.SaveChanges( );
                b2TableData = TryGetBackBlazeB2Data( tabledata.Id, sqliteContext );

            } else {
                b2TableData.FileID = fileId;
                b2TableData.BucketName = config.BucketName;
                b2TableData.BucketId = config.BucketId;
                sqliteContext.SaveChanges( );
            }
            tabledata.UsesBackBlazeB2 = true;
            sqliteContext.SaveChanges( );
            ReleaseSqliteContext( );

            s_logger?.ILog?.Info( "UploadFileToB2 DB Data:" );
            s_logger?.ILog?.Info( b2TableData );

            activity?.Stop( );
        }

    }
}
