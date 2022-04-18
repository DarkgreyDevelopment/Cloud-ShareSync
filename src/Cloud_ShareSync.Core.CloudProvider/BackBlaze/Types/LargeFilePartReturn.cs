namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class LargeFilePartReturn {
        internal int PartNumber { get; set; }
        internal string Sha1Hash { get; set; }
        internal int DataSize { get; set; }

        internal LargeFilePartReturn(
            int part,
            string hash,
            int size
        ) {
            PartNumber = part;
            Sha1Hash = hash;
            DataSize = size;
        }
    }
}
