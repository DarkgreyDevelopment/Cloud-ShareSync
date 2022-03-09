using Cloud_ShareSync.Core.Logging;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.Cryptography.FileEncryption;
using Cloud_ShareSync.Core.Compression;

namespace Cloud_ShareSync.Core.Configuration.Enums {
    /// <summary>
    /// The list of supported features that can be optionally enabled.
    /// To perform any active actions Cloud-ShareSync requires that at least one
    /// mode (<see cref="Backup"/>, <see cref="Restore"/>),
    /// CloudProvider (<see cref="BackBlazeB2"/>),
    /// and Database source (<see cref="Sqlite"/>, <see cref="Postgres"/>) be selected.
    /// </summary>
    [Flags]
    public enum Cloud_ShareSync_Features {
        /// <summary>
        /// <para>
        /// Enables application logging and telemetry via the <see cref="TelemetryLogger"/>.
        /// </para>
        /// When enabled log settings are controlled by the <see cref="Log4NetConfig"/>.
        /// </summary>
        Log4Net = 2,

        /// <summary>
        /// Enables <see cref="ManagedChaCha20Poly1305"/> file encryption before upload and
        /// file decryption after download.
        /// </summary>
        Encryption = 4,

        /// <summary>
        /// <para>
        /// Enables file compression before upload and file decompression after download.
        /// </para>
        /// <para>
        /// Compression is currently handled via the <see cref="CompressionIntermediary"/>.
        /// </para>
        /// When enabled compression settings are controlled by the <see cref="CompressionConfig"/>.
        /// </summary>
        Compression = 8,

        /// <summary>
        /// <para>
        /// Enables Cloud-ShareSync to use a local sqlite database.
        /// </para>
        /// <para>
        /// Cloud-ShareSync requires a database source of either <see cref="Sqlite"/> or <see cref="Postgres"/>.
        /// </para>
        /// Database settings are controlled by the <see cref="DatabaseConfig"/>.
        /// </summary>
        Sqlite = 16,

        /// <summary>
        /// <para>
        /// Enables upload and download to BackBlaze B2 Cloud Storage.
        /// </para>
        /// <para>
        /// Cloud-ShareSync requires a CloudProvider. Currently BackBlazeB2 is the only enabled CloudProvider.
        /// </para>
        /// When enabled backblaze settings are controlled by the <see cref="B2Config"/>.
        /// </summary>
        BackBlazeB2 = 32,

        /// <summary>
        /// <para>
        /// Enables backup mode.
        /// </para>
        /// <para>
        /// Cloud-ShareSync requires that at least one mode (<see cref="Backup"/>, <see cref="Restore"/>) be enabled. 
        /// </para>
        /// When enabled backup settings are controlled by the <see cref="BackupConfig"/>.
        /// </summary>
        Backup = 64,

        /// <summary>
        /// <para>
        /// Enables restore mode.
        /// </para>
        /// <para>
        /// Cloud-ShareSync requires that at least one mode (<see cref="Restore"/>, <see cref="Backup"/>) be enabled. 
        /// </para>
        /// When enabled restore settings are controlled by the <see cref="RestoreConfig"/>.
        /// </summary>
        Restore = 128,

        /// <summary>
        /// <para>
        /// Enables Cloud-ShareSync to use a separate postgres database.
        /// </para>
        /// <para>
        /// Cloud-ShareSync requires a database source of either <see cref="Postgres"/> or <see cref="Sqlite"/>.
        /// </para>
        /// Database settings are controlled by the <see cref="DatabaseConfig"/>.
        /// </summary>
        Postgres = 256,
    }
}
