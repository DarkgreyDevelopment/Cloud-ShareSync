using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    public class BackupConfig {
        public string RootFolder { get; set; }
        public string[] ExcludePaths { get; set; } = Array.Empty<string>( );
        public string WorkingDirectory { get; set; } = "";
        public bool Recurse { get; set; } = true;
        public bool EncryptBeforeUpload { get; set; }
        public bool CompressBeforeUpload { get; set; }
        public bool UniqueCompressionPasswords { get; set; }
        public bool ObfuscateUploadedFileNames { get; set; }

        /// <summary>
        /// Returns the <see cref="BackupConfig"/> as a json string.
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
