using System.Text.Json;
using System.Text.Json.Serialization;
using Cloud_ShareSync.Core.Database.Entities;

namespace Cloud_ShareSync.Core.BackgroundService.PrepFile {
    public class PrepItem {
        [JsonConverter( typeof( FileSystemInfoJsonConverter ) )]
        public FileInfo File { get; set; }
        public string RelativeFilePath { get; set; }
        public PrimaryTable? CoreData { get; set; }
        public BackBlazeB2Table? BackBlazeData { get; set; }

        public PrepItem( string path, string rootFolder ) {
            File = new( path );
            RelativeFilePath = GetFilePath( rootFolder );
        }

        public override string ToString( ) {
            return JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
        }

        private string GetFilePath( string? rootFolder ) => rootFolder != null
            ? Path.GetRelativePath( rootFolder, File.FullName )
            : File.Name;
    }
}
