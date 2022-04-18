namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
#nullable disable
    internal class B2FilesResponse {
#pragma warning disable IDE1006 // Naming Styles
        public B2FileResponse[] files { get; set; }
        public string nextFileId { get; set; }
        public string nextFileName { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
#nullable enable
}
