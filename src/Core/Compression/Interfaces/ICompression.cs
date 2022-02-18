namespace Cloud_ShareSync.Core.Compression.Interfaces {
    public interface ICompression {
        Task<FileInfo> CompressPath( FileSystemInfo path, FileInfo compressedPath, string? password );
        Task<FileSystemInfo> DecompressPath( FileInfo path, FileInfo decompressedPath, string? password );
    }
}
