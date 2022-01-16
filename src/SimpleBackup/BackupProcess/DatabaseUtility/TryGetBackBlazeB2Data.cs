using System.Diagnostics;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static BackBlazeB2Table? TryGetBackBlazeB2Data( string fileId ) {
            using Activity? activity = s_source.StartActivity( "TryGetBackBlazeB2Data" )?.Start( );

            BackBlazeB2Table? result = string.IsNullOrWhiteSpace( fileId ) ?
                                        null :
                                        s_sqlliteContext?.BackBlazeB2Data
                                            .Where( b => b.FileID == fileId )
                                            .FirstOrDefault( );

            activity?.Stop( );
            return result;
        }

        private static BackBlazeB2Table? TryGetBackBlazeB2Data( long? id ) {
            using Activity? activity = s_source.StartActivity( "TryGetBackBlazeB2Data" )?.Start( );

            BackBlazeB2Table? result = id == null ?
                                        null :
                                        s_sqlliteContext?.BackBlazeB2Data
                                            .Where( b => b.Id == id )
                                            .FirstOrDefault( );
            activity?.Stop( );
            return result;
        }

    }
}
