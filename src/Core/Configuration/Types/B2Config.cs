using System.Text.Json;
using Cloud_ShareSync.Core.Configuration.Interfaces;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// <para>
    /// Required configuration values to connect to a BackBlazeB2 bucket.
    /// </para>
    /// Related BackBlaze Documentation: <see href="https://help.backblaze.com/hc/en-us/articles/360052129034-Creating-and-Managing-Application-Keys"/>
    /// </summary>
    public class B2Config : ICloudProviderConfig {
        /// <summary>
        /// The "keyID" associated with the BackBlaze B2 <see cref="ApplicationKey"/>.
        /// </summary>
        public string ApplicationKeyId { get; set; }

        /// <summary>
        /// The value for the BackBlaze B2 api key.
        /// </summary>
        public string ApplicationKey { get; set; }

        /// <summary>
        /// The name of the BackBlaze B2 storage "bucket".
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// The id of the BackBlaze B2 storage "bucket".
        /// </summary>
        public string BucketId { get; set; }

        /// <summary>
        /// The number of consecutive errors to receive before aborting an upload/download.
        /// Default value is set to 10.
        /// </summary>
        public int MaxConsecutiveErrors { get; set; } = 10;

        /// <summary>
        /// The number of concurrent connections to open for large file uploads or downloads.
        /// Default value is set to 50.
        /// </summary>
        public int ProcessThreads { get; set; } = 50;

        /// <summary>
        /// Public Parameterless Constructor - Requires manual assignment of all non-default values.
        /// Used in the IConfiguration import process.
        /// </summary>
        public B2Config( ) { }

        /// <summary>
        /// Returns the <see cref="B2Config"/> as a json string.
        /// </summary>
        public override string ToString( ) =>
            JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions( ) {
                    IncludeFields = true,
                    WriteIndented = true,
                }
            );
    }
}
