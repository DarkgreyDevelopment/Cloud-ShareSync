using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    /// <summary>
    /// <para>
    /// Cloud-ShareSync has two modes of operation. Backup and Restore.
    /// </para>
    /// <para>
    /// Restore mode enables the download of files from Cloud-Providers to a local fileshare.
    /// All files will be downloaded to paths relative to the <see cref="RootFolder"/>.
    /// </para>
    /// </summary>
    public class RestoreConfig {
        /// <summary>
        /// The folder to use as the root directory for all downloaded files.
        /// </summary>
        public string RootFolder { get; set; }

        /// <summary>
        /// The working directory to use while processing downloaded files.
        /// Files will be decompressed or decrypted in this directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Returns the <see cref="RestoreConfig"/> as a json string.
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
#nullable enable
}
