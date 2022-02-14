namespace Cloud_ShareSync.Core.Compression.Interfaces {
    public interface ICompression {
        FileInfo CompressPath( FileSystemInfo path, string? password );
        FileSystemInfo DecompressPath( FileInfo path, string? password );
    }
}
