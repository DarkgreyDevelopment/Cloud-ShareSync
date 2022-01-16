using System.Text.Json;

namespace Cloud_ShareSync.Core.Configuration.Types {
#nullable disable
    public class BackupConfig {
        public string WorkingDirectory { get; set; }
        public string RootFolder { get; set; }
        public string[] ExcludePaths { get; set; }
        public bool MonitorSubDirectories { get; set; }
        public bool EncryptBeforeUpload { get; set; }
        public bool CompressBeforeUpload { get; set; }
        public bool UniqueCompressionPasswords { get; set; }
        public bool ObfuscateUploadedFileNames { get; set; }

        public override string ToString( ) {
            JsonSerializerOptions options = new( ) {
                IncludeFields = true,
                WriteIndented = true,
            };
            return JsonSerializer.Serialize( this, options );
        }
    }
#nullable enable
}
