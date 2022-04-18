namespace Cloud_ShareSync.Core.Compression.Interfaces {
    /// <summary>
    /// Represents a type used to perform compression/decompression. 
    /// </summary>
    public interface ICompression {
        /// <summary>
        /// Compresses the <paramref name="inputPath"/> and outputs it to <paramref name="compressedFilePath"/>.
        /// Optionally <paramref name="password"/> protect the compressed file.
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="compressedFilePath"></param>
        /// <param name="password"></param>
        /// <returns>The FileInfo object for the <paramref name="compressedFilePath"/>.</returns>
        Task<FileInfo> CompressPath( FileSystemInfo inputPath, FileInfo compressedFilePath, string? password );

        /// <summary>
        /// Decompresses the <paramref name="inputPath"/> and outputs the contents to the <paramref name="decompressionDir"/>.
        /// Optionally accepts a <paramref name="password"/> for the decompression process.
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="decompressionDir"></param>
        /// <param name="password"></param>
        /// <returns>An IEnumerable<FileSystemInfo> of decompressed objects.</returns>
        Task<IEnumerable<FileSystemInfo>> DecompressPath( FileInfo inputPath, DirectoryInfo decompressionDir, string? password );
    }
}
