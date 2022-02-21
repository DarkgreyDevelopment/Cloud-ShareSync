namespace Cloud_ShareSync.Core.Compression.Interfaces {
    public interface ICompression {
        Task<FileInfo> CompressPath( FileSystemInfo inputPath, FileInfo compressedPath, string? password );
        Task<IEnumerable<FileSystemInfo>> DecompressPath( FileInfo inputPath, DirectoryInfo decompressionDir, string? password );
    }
}
