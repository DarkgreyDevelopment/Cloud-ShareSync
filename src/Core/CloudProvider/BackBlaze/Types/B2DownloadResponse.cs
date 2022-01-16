using System.Text.Json;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
#nullable disable
    public class B2DownloadResponse {

        public FileInfo OutputPath { get; set; }
        public string FileID { get; set; }
        public string FileName { get; set; }
        public DateTime LastModified { get; set; }
        public string Sha512FileHash { get; set; }
        public string Sha1FileHash { get; set; }

        public override string ToString( ) {
            return JsonSerializer.Serialize(
                new TempJsonObj( this ),
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
        }

        private class TempJsonObj {
            public string OutputPath { get; set; }
            public string FileID { get; set; }
            public string FileName { get; set; }
            public string LastModified { get; set; }
            public string Sha512FileHash { get; set; }
            public string Sha1FileHash { get; set; }

            internal TempJsonObj( B2DownloadResponse obj ) {
                OutputPath = obj.OutputPath.FullName;
                FileID = obj.FileID;
                FileName = obj.FileName;
                LastModified = obj.LastModified.ToString( );
                Sha512FileHash = obj.Sha512FileHash;
                Sha1FileHash = obj.Sha1FileHash;
            }
        }
    }
#nullable enable
}
