using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.SharedServices.BackgroundService.Types {
    internal class PrepItem {
        public FileInfo UploadFile { get; set; }
        public string UploadPath { get; set; }
        public PrimaryTable? CoreData { get; set; }
        public BackBlazeB2Table? BackBlazeData { get; set; }
        public B2FileResponse? B2Response { get; set; }

        public PrepItem( string path, string rootFolder ) {
            UploadFile = new( path );
            UploadPath = Path.GetRelativePath( rootFolder, path );
        }

        public override string ToString( ) {
            return JsonSerializer.Serialize(
                new TmpJson( this ),
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
        }
        private class TmpJson {
            public string UploadFile { get; set; }
            public string UploadPath { get; set; }
            public PrimaryTable? CoreData { get; set; }
            public BackBlazeB2Table? BackBlazeData { get; set; }
            public B2FileResponse? B2Response { get; set; }

            public TmpJson( PrepItem item ) {
                UploadFile = item.UploadFile.FullName;
                UploadPath = item.UploadPath;
                CoreData = item.CoreData;
                BackBlazeData = item.BackBlazeData;
                B2Response = item.B2Response;
            }
        }
    }
}
