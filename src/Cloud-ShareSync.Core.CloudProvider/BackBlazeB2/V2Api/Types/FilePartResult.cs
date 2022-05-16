namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Types {
    internal class FilePartResult {

        internal FilePartResult(
            int part,
            int size,
            long offset
        ) {
            PartNumber = part;
            PartSize = size;
            Offset = offset;
        }

        internal int PartNumber { get; private set; }
        internal string PartSha1 { get; set; } = string.Empty;
        internal int PartSize { get; private set; }
        internal long Offset { get; private set; }

        public KeyValuePair<int, string> NewPartKVP( ) => new( PartNumber, PartSha1 );
    }
}
