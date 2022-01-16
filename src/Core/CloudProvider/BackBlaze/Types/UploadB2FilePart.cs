namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    public class UploadB2FilePart {

        public string UploadUrl { get; set; }
        public string AuthorizationToken { get; set; }
        public string MimeType { get; set; }
        public string CompleteSha1Hash { get; set; }
        public string PartSha1Hash { get; set; }
        public string PartNumber { get; set; }
        public byte[] Content { get; set; }

        internal UploadB2FilePart(
            UploadB2File uploadObject,
            string partSha1Hash,
            int partNumber,
            byte[] content
        ) {
            UploadUrl = uploadObject.UploadUrl;
            AuthorizationToken = uploadObject.AuthorizationToken;
            MimeType = uploadObject.MimeType;
            CompleteSha1Hash = uploadObject.CompleteSha1Hash;
            PartSha1Hash = partSha1Hash;
            PartNumber = partNumber.ToString( );
            Content = content;
        }
    }
}
