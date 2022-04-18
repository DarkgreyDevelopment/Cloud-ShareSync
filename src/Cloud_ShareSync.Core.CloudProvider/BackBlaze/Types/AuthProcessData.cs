namespace Cloud_ShareSync.Core.CloudProvider.BackBlaze.Types {
    internal class AuthProcessData {
        private static readonly NullReferenceException s_accountId = new( nameof( AccountId ) );
        private static readonly NullReferenceException s_apiUrl = new( nameof( ApiUrl ) );
        private static readonly NullReferenceException s_s3ApiUrl = new( nameof( S3ApiUrl ) );
        private static readonly NullReferenceException s_downloadUrl = new( nameof( DownloadUrl ) );
        private static readonly NullReferenceException s_recommendedPartSize = new( nameof( RecommendedPartSize ) );
        private static readonly NullReferenceException s_absoluteMinimumPartSize = new( nameof( AbsoluteMinimumPartSize ) );

        internal string AccountId { get; set; }
        internal string ApiUrl { get; set; }
        internal string S3ApiUrl { get; set; }
        internal string DownloadUrl { get; set; }
        internal int RecommendedPartSize { get; set; }
        internal int AbsoluteMinimumPartSize { get; set; }

        internal AuthProcessData(
            string? accountId,
            string? apiUrl,
            string? s3ApiUrl,
            string? downloadUrl,
            int? recommendedPartSize,
            int? absoluteMinimumPartSize
        ) {
            AccountId = ValidateAccountID( accountId );
            ApiUrl = ValidateApiUrl( apiUrl );
            S3ApiUrl = ValidateS3ApiUrl( s3ApiUrl );
            DownloadUrl = ValidateDownloadUrl( downloadUrl );
            RecommendedPartSize = ValidateRecommendedPartSize( recommendedPartSize );
            AbsoluteMinimumPartSize = ValidateAbsoluteMinimumPartSize( absoluteMinimumPartSize );
        }

        private static string ValidateAccountID( string? accountId ) =>
            accountId ?? throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    s_accountId
                );

        private static string ValidateApiUrl( string? apiUrl ) =>
            apiUrl ?? throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    s_apiUrl
                );

        private static string ValidateS3ApiUrl( string? s3ApiUrl ) =>
            s3ApiUrl ?? throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    s_s3ApiUrl
                );

        private static string ValidateDownloadUrl( string? downloadUrl ) =>
            downloadUrl ?? throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    s_downloadUrl
                );

        private static int ValidateRecommendedPartSize( int? recommendedPartSize ) =>
            recommendedPartSize is not null and not 0 ?
                (int)recommendedPartSize :
                throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    s_recommendedPartSize
                );

        private static int ValidateAbsoluteMinimumPartSize( int? absoluteMinimumPartSize ) =>
            absoluteMinimumPartSize != null ?
                (int)absoluteMinimumPartSize :
                throw new InvalidB2Response(
                    B2.AuthorizationURI,
                    s_absoluteMinimumPartSize
                );
    }
}
