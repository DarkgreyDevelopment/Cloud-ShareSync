namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    public class FilePartInfo {
        public int PartNumber { get; set; }
        public int ContentLength { get; set; }
        public string Sha1Hash { get; set; }
        public byte[] Data { get; set; }

        internal FilePartInfo(
            int partNumber,
            int contentLength
        ) {
            PartNumber = partNumber;
            ContentLength = contentLength;
            Sha1Hash = "";
            Data = new byte[contentLength];
        }
    }
}
