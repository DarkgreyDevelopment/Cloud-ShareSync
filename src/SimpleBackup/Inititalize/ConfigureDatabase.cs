﻿using System.Diagnostics;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void ConfigureDatabase( ) {
            using Activity? activity = s_source.StartActivity( "Inititalize.ConfigureDatabase" )?.Start( );

            s_sqlliteContext = (s_config?.Database != null) ?
                new( s_config.Database.SqlliteDBPath ) :
                throw new InvalidDataException( "Database configuration required." );

            int coreTableCount = (from obj in s_sqlliteContext.CoreData where obj.Id >= 0 select obj).Count( );
            int encryptedCount = (from obj in s_sqlliteContext.EncryptionData where obj.Id >= 0 select obj).Count( );
            int compressdCount = (from obj in s_sqlliteContext.CompressionData where obj.Id >= 0 select obj).Count( );
            int backBlazeCount = (from obj in s_sqlliteContext.BackBlazeB2Data where obj.Id >= 0 select obj).Count( );
            s_logger?.ILog?.Info( "Database Inititalized." );
            s_logger?.ILog?.Info( $"Core Table      : {coreTableCount}" );
            s_logger?.ILog?.Info( $"Encrypted Table : {encryptedCount}" );
            s_logger?.ILog?.Info( $"Compressed Table: {compressdCount}" );
            s_logger?.ILog?.Info( $"BackBlaze Table : {backBlazeCount}" );

            activity?.Stop( );
        }

    }
}
