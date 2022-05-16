using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Endpoints {
    public class DownloadFileById {
        [JsonIgnore]
        public HttpResponseMessage? Response { get; set; }
        public long ContentLength { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? ContentSha1 { get; set; }
        public string? AcceptRanges { get; set; }
        public Dictionary<string, string> Info { get; set; } = new( );
        public Dictionary<string, string> B2ContentHeaders { get; set; } = new( );
        public DateTime UploadTime { get; set; }
        public DateTime DownloadTime { get; set; }

        public const string EndpointURI = "/b2api/v2/b2_download_file_by_id";

        public override string ToString( ) {
            return JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
        }

        internal static async Task<DownloadFileById> CallApi(
            HttpMethod method,
            string downloadUrl,
            string authToken,
            string fileId,
            string? range,
            HttpClient client,
            ILogger? log = null
        ) {
            string downloadUri = downloadUrl + EndpointURI;
            string? body = $"{{\"fileId\":\"{fileId}\"}}";

            DownloadFileById result = new( );
            if (method == HttpMethod.Head || method == HttpMethod.Get) {
                downloadUri += $"?fileId={fileId}";
                body = null;
            }

            result.Response = await B2RequestHandler.SendB2Request(
               method,
               downloadUri,
               authToken,
               body,
               null,
               range,
               false,
               client
           );
            GetDownloadResponseValues( result, log );
            return result;
        }

        private static void GetDownloadResponseValues(
            DownloadFileById downloadInfo,
            ILogger? log
        ) {
            HttpResponseMessage responseMessage = downloadInfo.Response!;
            if (responseMessage.Headers.Date != null) {
                downloadInfo.DownloadTime = ((DateTimeOffset)responseMessage.Headers.Date).DateTime;
            }
            downloadInfo.ContentLength = responseMessage.Content.Headers.ContentLength ?? 0;
            downloadInfo.ContentType = responseMessage.Content.Headers?.ContentType?.MediaType ?? string.Empty;
            ExtractHeaderValues( responseMessage.Headers.Distinct( ), downloadInfo );
            log?.LogDebug( "{string}", downloadInfo.ToString( ) );
        }

        private static readonly Regex s_fileNameRegex = new( "x-bz-file-name", RegexOptions.Compiled | RegexOptions.IgnoreCase );
        private static readonly Regex s_fileIdRegex = new( "x-bz-file-id", RegexOptions.Compiled | RegexOptions.IgnoreCase );
        private static readonly Regex s_contentSha1Regex = new( "x-bz-content-sha1", RegexOptions.Compiled | RegexOptions.IgnoreCase );
        private static readonly Regex s_uploadTimestampRegex = new( "X-Bz-Upload-Timestamp", RegexOptions.Compiled | RegexOptions.IgnoreCase );
        private static readonly Regex s_acceptRangesRegex = new( "Accept-Ranges", RegexOptions.Compiled | RegexOptions.IgnoreCase );
        private static readonly Regex s_infoRegex = new( "x-bz-info-", RegexOptions.Compiled | RegexOptions.IgnoreCase );
        private static readonly Regex s_b2Regex = new( "x-bz-", RegexOptions.Compiled | RegexOptions.IgnoreCase );

        private static void ExtractHeaderValues(
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
            DownloadFileById downloadInfo
        ) {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers) {
                switch (true) {
                    case true when s_fileNameRegex.Match( header.Key ).Success:
                        downloadInfo.FileName = header.Value.First( );
                        break;
                    case true when s_fileIdRegex.Match( header.Key ).Success:
                        downloadInfo.FileId = header.Value.First( );
                        break;
                    case true when s_contentSha1Regex.Match( header.Key ).Success:
                        downloadInfo.ContentSha1 = header.Value.First( );
                        break;
                    case true when s_uploadTimestampRegex.Match( header.Key ).Success:
                        downloadInfo.UploadTime = DateTimeOffset.FromUnixTimeMilliseconds( long.Parse( header.Value.First( ) ) ).UtcDateTime;
                        break;
                    case true when s_acceptRangesRegex.Match( header.Key ).Success:
                        downloadInfo.AcceptRanges = header.Value.First( );
                        break;
                    case true when s_infoRegex.Match( header.Key ).Success:
                        downloadInfo.Info.Add( header.Key.ToLower( ), header.Value.First( ) );
                        break;
                    case true when s_b2Regex.Match( header.Key ).Success:
                        downloadInfo.B2ContentHeaders.Add( header.Key, header.Value.First( ) );
                        break;
                }
            }
        }

    }
}
