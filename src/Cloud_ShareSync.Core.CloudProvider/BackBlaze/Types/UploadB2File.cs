using System.Text.Json;
using Cloud_ShareSync.Core.CloudProvider.Interfaces;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    public class UploadB2File : ICloudProviderUpload {

        public UploadB2File(
            FileInfo filePath,
            string uploadFilePath,
            string originalFileName,
            string completeSha512Hash,
            string completeSHA1Hash,
            string mimeType
        ) {
            FilePath = filePath;
            OriginalFileName = string.IsNullOrWhiteSpace( originalFileName ) ? filePath.FullName : originalFileName;
            UploadFilePath = string.IsNullOrWhiteSpace( uploadFilePath ) ? filePath.Name : uploadFilePath;
            CompleteSha1Hash = completeSHA1Hash;
            CompleteSha512Hash = completeSha512Hash;
            MimeType = mimeType;
            Sha1PartsList = new( );
        }

        public UploadB2File(
            FileInfo filePath,
            string? originalFileName = null,
            string? uploadFilePath = null,
            string? sha512Hash = null
        ) : this(
                filePath: filePath,
                originalFileName: originalFileName ?? "",
                uploadFilePath: uploadFilePath ?? "",
                completeSha512Hash: sha512Hash ?? "",
                completeSHA1Hash: "",
                mimeType: ""
        ) { }

        public FileInfo FilePath { get; set; }
        public string OriginalFileName { get; set; }
        public string UploadFilePath { get; set; }
        public string MimeType { get; set; }
        public string CompleteSha512Hash { get; set; }
        public string CompleteSha1Hash { get; set; }
        public List<KeyValuePair<int, string>> Sha1PartsList { get; set; }
        public string FileId { get; set; } = "";
        public string UploadUrl { get; set; } = "";
        public string AuthorizationToken { get; set; } = "";
        public long TotalBytesSent { get; set; }

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
            public string FilePath { get; set; }
            public string UploadName { get; set; }
            public string MimeType { get; set; }
            public string CompleteSha512Hash { get; set; }
            public string CompleteSha1Hash { get; set; }
            public KeyValuePair<int, string>[] Sha1PartsList { get; set; }
            public string FileId { get; set; }
            public string UploadUrl { get; set; }
            public string AuthorizationToken { get; set; }
            public long TotalBytesSent { get; set; }

            internal TempJsonObj( UploadB2File obj ) {
                FilePath = obj.FilePath.FullName;
                UploadName = obj.UploadFilePath;
                MimeType = obj.MimeType;
                CompleteSha512Hash = obj.CompleteSha512Hash;
                CompleteSha1Hash = obj.CompleteSha1Hash;
                Sha1PartsList = obj.Sha1PartsList.ToArray( );
                FileId = obj.FileId;
                UploadUrl = obj.UploadUrl;
                AuthorizationToken = obj.AuthorizationToken;
                TotalBytesSent = obj.TotalBytesSent;
            }
        }
    }
}
