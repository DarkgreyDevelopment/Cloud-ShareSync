namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class AuthProcessData {
        internal string? AccountId { get; set; }
        internal string? ApiUrl { get; set; }
        internal string? S3ApiUrl { get; set; }
        internal string? DownloadUrl { get; set; }
        internal int? RecommendedPartSize { get; set; }
        internal int? AbsoluteMinimumPartSize { get; set; }

        internal AuthProcessData(
            string? accountId,
            string? apiUrl,
            string? s3ApiUrl,
            string? downloadUrl,
            int? recommendedPartSize,
            int? absoluteMinimumPartSize
        ) {
            AccountId = accountId;
            ApiUrl = apiUrl;
            S3ApiUrl = s3ApiUrl;
            DownloadUrl = downloadUrl;
            RecommendedPartSize = recommendedPartSize;
            AbsoluteMinimumPartSize = absoluteMinimumPartSize;
        }

        internal AuthProcessData( ) {
            AccountId = null;
            ApiUrl = null;
            S3ApiUrl = null;
            DownloadUrl = null;
            RecommendedPartSize = null;
            AbsoluteMinimumPartSize = null;
        }

        internal void ValidateNotNull( ) {
            if (AccountId == null) {
                throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    new NullReferenceException( nameof( AccountId ) )
                );
            }
            if (ApiUrl == null) {
                throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    new NullReferenceException( nameof( ApiUrl ) )
                );
            }
            if (S3ApiUrl == null) {
                throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    new NullReferenceException( nameof( S3ApiUrl ) )
                );
            }
            if (DownloadUrl == null) {
                throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    new NullReferenceException( nameof( DownloadUrl ) )
                );
            }
            if (RecommendedPartSize == null) {
                throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    new NullReferenceException( nameof( RecommendedPartSize ) )
                );
            }
            if (AbsoluteMinimumPartSize == null) {
                throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    new NullReferenceException( nameof( AbsoluteMinimumPartSize ) )
                );
            }
        }
    }
}
