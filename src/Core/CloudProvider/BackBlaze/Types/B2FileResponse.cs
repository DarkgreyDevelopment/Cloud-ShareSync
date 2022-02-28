using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
#nullable disable
    internal class B2FileResponse {

#pragma warning disable IDE1006 // Naming Styles - These match the parameter names from backblaze.
        [JsonInclude]
        public string accountId { get; set; }
        [JsonInclude]
        public string action { get; set; }
        [JsonInclude]
        public string bucketId { get; set; }
        [JsonInclude]
        public long contentLength { get; set; }
        [JsonInclude]
        public string contentSha1 { get; set; }
        [JsonInclude]
        public string contentType { get; set; }
        [JsonInclude]
        public string fileId { get; set; }
        [JsonInclude]
        public Dictionary<string, string> fileInfo { get; set; }
        [JsonInclude]
        public string fileName { get; set; }
        [JsonInclude]
        public long uploadTimestamp { get; set; }
#pragma warning restore IDE1006 // Naming Styles - These match the parameter names from backblaze.
        [JsonInclude]
        public DateTime? DatetimeTimeStamp => (uploadTimestamp == 0) ?
                                            null :
                                            DateTimeOffset.FromUnixTimeMilliseconds( this.uploadTimestamp ).DateTime;

        public override string ToString( ) {
            JsonSerializerOptions options = new( ) {
                IncludeFields = true,
                WriteIndented = true,
            };
            return JsonSerializer.Serialize( this, options );
        }
    }
}
