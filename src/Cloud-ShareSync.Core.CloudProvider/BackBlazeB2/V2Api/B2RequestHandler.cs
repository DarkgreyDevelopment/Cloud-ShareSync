using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Enums;
using Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api.Exceptions;
using Cloud_ShareSync.Core.CloudProvider.SharedServices;
using Microsoft.Extensions.Logging;
using Polly.Contrib.WaitAndRetry;

namespace Cloud_ShareSync.Core.CloudProvider.BackBlazeB2.V2Api {
    internal static class B2RequestHandler {

        #region GetJitterBackOffTimeSpans

        public static TimeSpan[] GetJitterBackOffTimeSpans( int retryCount ) =>
            Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: TimeSpan.FromSeconds( 1 ),
                retryCount: retryCount,
                seed: null, // Use Default random seed
                fastFirst: false // Wait before retrying
            ).ToArray( );

        #endregion GetJitterBackOffTimeSpans


        #region EnsureB2SuccessResponse

        private static bool EnsureB2SuccessResponse(
            this HttpResponseMessage result,
            EndpointCalls call,
            TimeSpan[] timeSpans,
            int attemptNumber,
            ILogger? log = null
        ) {
            try {
                _ = result.EnsureSuccessStatusCode( );
            } catch (HttpRequestException ex) {
                TimeSpan sleepTime = GetSleepTime( result, timeSpans, attemptNumber - 1 );
                bool restartUploadFileFailure = ex.CheckRestartUploadFileFailure( );
                if (attemptNumber < timeSpans.Length) {
                    log?.LogWarning(
                        "An error has occurred while making a B2 api request. Attempt#{int}. Error: {string}",
                        attemptNumber,
                        ex.Message
                    );
                    Sleep( sleepTime, log );
                }
                Exception? outputExp = true switch {
                    true when restartUploadFileFailure && call == EndpointCalls.UploadPart => new RestartUploadPartException( ex.Message, ex ),
                    true when restartUploadFileFailure && call == EndpointCalls.UploadFile => new RestartUploadFileException( ex.Message, ex ),
                    true when ex.ResetUnauthorizedFailure( call ) => new NewAuthTokenRequiredException( ex.Message, ex ),
                    _ => new RestartB2RequestException( ex.Message, ex ),
                };
                throw outputExp;
            }
            return true;
        }

        private static TimeSpan GetSleepTime(
            HttpResponseMessage result,
            TimeSpan[] timeSpans,
            int attemptNumber
        ) =>
            TimeSpan.FromMilliseconds(
                Math.Max(
                    timeSpans[attemptNumber].TotalMilliseconds,
                    GetServerWaitDuration( result ).TotalMilliseconds
                )
            );


        private static bool ResetUnauthorizedFailure( this HttpRequestException ex, EndpointCalls call ) =>
            ex.StatusCode == HttpStatusCode.Unauthorized &&
            call != EndpointCalls.UploadFile &&
            call != EndpointCalls.UploadPart;

        private static bool CheckRestartUploadFileFailure( this HttpRequestException ex ) =>
            (ex.StatusCode == null ? 0 : (int)ex.StatusCode) >= 500 ||
            ex.StatusCode == HttpStatusCode.RequestTimeout ||
            ex.StatusCode == HttpStatusCode.TooManyRequests ||
            MatchRegexPattern1( ex.Message ) ||
            MatchRegexPattern2( ex.Message );

        private static readonly Regex s_errMsg1 = new( "A connection attempt failed because the connected party did not properly respond", RegexOptions.Compiled );
        private static bool MatchRegexPattern1( string msg ) => s_errMsg1.Match( msg ).Success;

        private static readonly Regex s_errMsg2 = new( "^Error while copying content to a stream.$", RegexOptions.Compiled );
        private static bool MatchRegexPattern2( string msg ) => s_errMsg2.Match( msg ).Success;

        private static void Sleep( TimeSpan sleepTime, ILogger? log ) {
            log?.LogInformation( "Sleeping for {double} seconds.", sleepTime.TotalSeconds );
            Thread.Sleep( sleepTime );
        }

        private static TimeSpan GetServerWaitDuration( HttpResponseMessage response ) {
            RetryConditionHeaderValue? retryAfter = response?.Headers?.RetryAfter;
            return retryAfter == null ?
                TimeSpan.FromSeconds( 1 ) :
                    retryAfter.Date.HasValue ?
                        retryAfter.Date.Value - DateTime.UtcNow :
                        retryAfter.Delta.GetValueOrDefault( TimeSpan.FromSeconds( 1 ) );
        }

        #endregion EnsureB2SuccessResponse


        #region SendB2Request

        internal static async Task<HttpResponseMessage> SendB2Request(
            HttpMethod method,
            string uri,
            string authToken,
            object? content,
            List<KeyValuePair<string, string>>? contentHeaders,
            string? range,
            bool readResponseContent,
            HttpClient client
        ) => await NewHttpRequestMessage(
                method, uri, authToken, content, contentHeaders, range
            ).SendAsyncRequest(
                client, readResponseContent
            );

        private static B2HttpRequestMessage NewHttpRequestMessage(
            HttpMethod method,
            string uri,
            string authToken,
            object? content,
            List<KeyValuePair<string, string>>? contentHeaders,
            string? range
        ) => (content == null || content.GetType( ) == typeof( string ) || content.GetType( ) == typeof( byte[] )) == false ?
            throw new ArgumentException( "content must be null or of type string or byte[].", nameof( content ) ) :
            content == null ?
                new( method, uri, authToken, range ) :
                content.GetType( ) == typeof( string ) ?
                    new( method, uri, authToken, (string)content, contentHeaders, range ) :
                    new( method, uri, authToken, (byte[])content, contentHeaders, range );

        #endregion SendB2Request


        #region ProcessB2Request

        internal static async Task<JsonDocument> ProcessB2Request(
            string uri,
            string authToken,
            byte[] content,
            List<KeyValuePair<string, string>>? contentHeaders,
            HttpClient client,
            EndpointCalls call,
            int attemptNumber,
            TimeSpan[] timeSpans,
            ILogger? log
        ) {
            using HttpResponseMessage result = await SendB2Request(
                HttpMethod.Post, uri, authToken, content, contentHeaders, null, true, client
            );
            _ = result.EnsureB2SuccessResponse( call, timeSpans, attemptNumber, log );
            return result.ReadJsonContentStream( uri, call.ToString( ), log )!;
        }

        internal static async Task<JsonDocument> ProcessB2Request(
            HttpMethod method,
            string uri,
            string authToken,
            object? content,
            List<KeyValuePair<string, string>>? contentHeaders,
            EndpointCalls call,
            int retryCount,
            HttpClient client,
            bool readResponseContent,
            ILogger? log
        ) {
            TimeSpan[] timeSpans = GetJitterBackOffTimeSpans( retryCount );
            int count = 0;
            bool success = false;
            RestartB2RequestException? ex = null;
            do {
                count++;
                using HttpResponseMessage result = await SendB2Request(
                    method, uri, authToken, content, contentHeaders, null, readResponseContent, client
                );
                try {
                    success = result.EnsureB2SuccessResponse( call, timeSpans, count, log );
                } catch (RestartB2RequestException e) {
                    ex = e;
                    continue;
                }
                return result.ReadJsonContentStream( uri, call.ToString( ), log )!;
            } while (success != true && count < retryCount);
            throw new FailedB2RequestException( $"Failed while calling '{uri}' after {retryCount} attempts.", ex );
        }

        #endregion ProcessB2Request

    }
}
