using System.Diagnostics;
using Cloud_ShareSync.Core.Configuration.Types;

namespace Cloud_ShareSync.SimpleBackup {

    public partial class Program {

        private static void DeleteWorkingFile(
            FileInfo uploadFile,
            BackupConfig config
        ) {
            using Activity? activity = s_source.StartActivity( "DeleteWorkingDirFile" )?.Start( );

            if (config.EncryptBeforeUpload || config.CompressBeforeUpload) { uploadFile.Delete( ); }

            activity?.Stop( );
        }

    }
}
