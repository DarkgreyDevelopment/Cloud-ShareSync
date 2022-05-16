using System.Diagnostics;
using Cloud_ShareSync.Core.Compression.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Compression {

    /// <summary>
    /// This class is used to compress/decompress files.
    /// Requires 7zip.
    /// </summary>
    public class ManagedCompression : ICompression {

        private static readonly ActivitySource s_source = new( "CompressionInterface" );
        private readonly FileInfo _dependencyPath;
        private readonly ILogger? _log;
        private readonly SemaphoreSlim _semaphore = new( 0, 1 );

        private readonly List<FailedToZipException> _exceptions = new( );

        public static bool PlatformSupported => OperatingSystem.IsWindows( ) || OperatingSystem.IsLinux( );

        /// <summary>
        /// Pass in the 7zip dependency path via the <paramref name="config"/>.
        /// Optionally set <paramref name="log"/> to enable logging.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="log"></param>
        public ManagedCompression( string dependencyPath, ILogger? log = null ) {
            if (PlatformSupported == false) {
                throw new PlatformNotSupportedException(
                    "Compression isn't supported on this platform at this time."
                );
            }
            _log = log;
            _dependencyPath = new( dependencyPath );
            _ = _semaphore.Release( 1 );
        }

        #region DecompressPath

        /// <summary>
        /// ICompression interface method used to decompress 7z files using <see cref="ManagedCompression"/>.
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="decompressedPath"></param>
        /// <param name="password"></param>
        /// <returns>An enumeration of the decompressed FileSystemInfo objects</returns>
        public async Task<IEnumerable<FileSystemInfo>> DecompressPath(
            FileInfo inputPath,
            DirectoryInfo decompressionDir,
            string? password
        ) => await DecompressPath( inputPath, decompressionDir, _dependencyPath, password );

        internal async Task<IEnumerable<FileSystemInfo>> DecompressPath(
            FileInfo inputPath,
            DirectoryInfo decompressionDir,
            FileInfo dependencyPath,
            string? password = null
        ) {
            using Activity? activity = s_source.StartActivity( "DecompressPath" )?.Start( );

            DirectoryInfo temporaryExtractionDir = GetTemporaryDirectory( decompressionDir.FullName );

            await RunDecompressProcess(
                decompressionDir.FullName,
                inputPath.FullName,
                temporaryExtractionDir.FullName,
                dependencyPath.FullName,
                password
            );

            List<FileSystemInfo> result = FinishDecompressProcess(
                decompressionDir.FullName,
                inputPath.FullName,
                temporaryExtractionDir.FullName
            );

            activity?.Stop( );
            return result;
        }

        #region DecompressProcess

        private async Task RunDecompressProcess(
            string outputDir,
            string inputPath,
            string temporaryExtractionDir,
            string dependencyPath,
            string? password
        ) {
            await _semaphore.WaitAsync( );
            _log?.LogInformation(
                "Extracting '{string}' into '{string}'.",
                inputPath,
                outputDir
            );
            Process process = NewDecompressProcess(
                inputPath,
                outputDir,
                dependencyPath,
                temporaryExtractionDir,
                password
            );
            RunProcess( process );
            _ = _semaphore.Release( );
        }

        private Process NewDecompressProcess(
            string inputPath,
            string outputPath,
            string dependencyPath,
            string workingDirectory,
            string? password = null
        ) {
            using Activity? activity = s_source.StartActivity( "DecompressProcess" )?.Start( );

            // 7z Cmdline Arguments - Works on both linux + windows.
            string arguments = $"x \"{inputPath}\" -o\"{outputPath}\"";
            if (string.IsNullOrWhiteSpace( password )) { arguments += $" -p\"{password}\""; }

            // Create Process
            Process process = Create7ZProcess( arguments, dependencyPath, workingDirectory );

            activity?.Stop( );
            return process;
        }

        private List<FileSystemInfo> FinishDecompressProcess(
            string decompressionDir,
            string inputPath,
            string temporaryExtractionDir
        ) {
            List<FileSystemInfo> decompressedItems = GetDecompressedItems(
                decompressionDir,
                inputPath
            );
            return CleanupTemporaryDirectory(
                temporaryExtractionDir,
                decompressionDir,
                decompressedItems
            );
        }

        #endregion DecompressProcess

        #region DecompressHelpers

        internal static DirectoryInfo GetTemporaryDirectory( string inputDir ) =>
            Directory.CreateDirectory(
                Path.Combine(
                    inputDir,
                    Path.GetRandomFileName( )
                )
            );

        internal List<FileSystemInfo> GetDecompressedItems( string decompressionDir, string inputPath ) {
            IEnumerable<string> decompressedItems = GetFileSystemEntries( decompressionDir );

            List<FileSystemInfo> result = new( );
            foreach (string item in decompressedItems) {
                result.Add( DeriveFileSystemInfoType( item ) );
            }

            _log?.LogInformation(
                "Successfully extracted '{string}'. \n" +
                "Extracted Item Count: '{string}'. \n",
                inputPath,
                decompressedItems.Count( )
            );
            return result;
        }

        private static IEnumerable<string> GetFileSystemEntries( string path ) {
            IEnumerable<string> existingItems = Enumerable.Empty<string>( );
            try {
                existingItems = Directory.EnumerateFileSystemEntries( path );
            } catch { }
            return existingItems;
        }

        private static FileSystemInfo DeriveFileSystemInfoType( string path ) =>
            File.Exists( path ) ? new FileInfo( path ) : new DirectoryInfo( path );

        #endregion DecompressHelpers

        #region Cleanup Temporary Directory

        internal static List<FileSystemInfo> CleanupTemporaryDirectory(
            string temporaryExtractionDir,
            string outputDir,
            List<FileSystemInfo> decompressedItems
        ) {
            IEnumerable<DirectoryInfo> decompressedDirs = CreateOutputDirectoryStructure(
                outputDir,
                temporaryExtractionDir,
                decompressedItems
            );
            IEnumerable<FileInfo> decompressedFiles = MoveFilesToOutputDirectory(
                outputDir,
                temporaryExtractionDir,
                decompressedItems
            );
            Directory.Delete( temporaryExtractionDir, true );
            return CombineResults( decompressedDirs, decompressedFiles );
        }

        internal static List<FileSystemInfo> CombineResults(
            IEnumerable<DirectoryInfo> decompressedDirs,
            IEnumerable<FileInfo> decompressedFiles
        ) {
            List<FileSystemInfo> result = new( );
            result.AddRange( decompressedDirs );
            result.AddRange( decompressedFiles );
            result.Sort(
                delegate ( FileSystemInfo x, FileSystemInfo y ) {
                    return x.FullName.CompareTo( y.FullName );
                }
            );
            return result;
        }

        internal static IEnumerable<FileInfo> MoveFilesToOutputDirectory(
            string outputDir,
            string temporaryExtractionDir,
            List<FileSystemInfo> decompressedItems
        ) {
            IEnumerable<Tuple<string, string>> relativePaths = GetRelativePaths(
                temporaryExtractionDir,
                GetFilePaths( decompressedItems )
            );
            foreach (Tuple<string, string> file in relativePaths) {
                string path = Path.Join( outputDir, file.Item2 );
                File.Move( file.Item1, path );
                yield return new FileInfo( path );
            }
        }

        internal static IEnumerable<DirectoryInfo> CreateOutputDirectoryStructure(
            string outputDir,
            string temporaryExtractionDir,
            List<FileSystemInfo> decompressedItems
        ) {
            return CreateOutputDirectories(
                outputDir,
                GetRelativePaths(
                    temporaryExtractionDir,
                    GetUniqueDirectoryPaths( decompressedItems )
                )
            );
        }

        internal static IEnumerable<DirectoryInfo> CreateOutputDirectories(
            string outputDir,
            IEnumerable<Tuple<string, string>> dirPaths
        ) {
            foreach (Tuple<string, string> relativePath in dirPaths) {
                string path = Path.Join( outputDir, relativePath.Item2 );
                if (Directory.Exists( path ) == false) {
                    _ = Directory.CreateDirectory( path );
                }
                yield return new DirectoryInfo( path );
            }
        }

        internal static IEnumerable<Tuple<string, string>> GetRelativePaths(
            string relativeRoot,
            IEnumerable<string> paths
        ) {
            foreach (string path in paths) {
                yield return new( path, path.Replace( relativeRoot, "" ) );
            }
        }

        internal static IEnumerable<string> GetFilePaths( List<FileSystemInfo> decompressedItems ) =>
            decompressedItems.Where( a => a is FileInfo ).Select( a => a.FullName ).Distinct( );

        internal static List<string> GetUniqueDirectoryPaths( List<FileSystemInfo> decompressedItems ) =>
            decompressedItems.Where( a => a is DirectoryInfo ).Select( a => a.FullName ).Distinct( ).ToList( );

        #endregion Cleanup Temporary Directory

        #endregion DecompressPath


        #region CompressPath

        /// <summary>
        /// ICompression interface method to compress files using 7zip via the <see cref="ManagedCompression"/>.
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="compressedFilePath"></param>
        /// <param name="password"></param>
        /// <returns>The fileinfo object of the compressed 7z file.</returns>
        public async Task<FileInfo> CompressPath(
            FileSystemInfo inputPath,
            FileInfo compressedFilePath,
            string? password = null
        ) => await CompressPath( inputPath, compressedFilePath, _dependencyPath, password );

        internal async Task<FileInfo> CompressPath(
            FileSystemInfo inputPath,
            FileInfo compressedPath,
            FileInfo dependencyPath,
            string? password = null
        ) {
            using Activity? activity = s_source.StartActivity( "CompressPath" )?.Start( );
            await _semaphore.WaitAsync( );
            RunCompressProcess(
                inputPath.FullName,
                compressedPath,
                dependencyPath.FullName,
                password
            );
            _ = _semaphore.Release( );
            activity?.Stop( );
            return compressedPath;
        }

        #region CompressProcess

        private void RunCompressProcess(
            string inputPath,
            FileInfo compressedPath,
            string dependencyPath,
            string? password
        ) {
            _log?.LogInformation(
                "Compressing File '{string}' into '{string}'.",
                inputPath,
                compressedPath.FullName
            );
            Process process = NewCompressProcess(
                inputPath,
                compressedPath.FullName,
                dependencyPath,
                compressedPath.Directory?.FullName ?? Path.GetTempPath( ),
                password
            );
            RunProcess( process );
            _log?.LogInformation(
                "Successfully compressed '{string}'. \n" +
                "Compressed FilePath: '{string}'. \n" +
                "Compressed FileSize: {long}.",
                inputPath,
                compressedPath.FullName,
                compressedPath.Length
            );
        }

        private Process NewCompressProcess(
            string inputPath,
            string outputPath,
            string dependencyPath,
            string workingDirectory,
            string? password = null
        ) {
            using Activity? activity = s_source.StartActivity( "CompressProcess" )?.Start( );

            // 7z Cmdline Arguments - Works on both linux + windows.
            string arguments = $"a \"{outputPath}\" -mfb=257 -mx=9 -mhe=on -mmt=on ";

            arguments += string.IsNullOrWhiteSpace( password ) ?
                                $"-- \"{inputPath}\"" :
                                $"-p\"{password}\" -- \"{inputPath}\"";
            // Create Process
            Process process = Create7ZProcess( arguments, dependencyPath, workingDirectory );

            activity?.Stop( );
            return process;
        }

        #endregion CompressProcess

        #endregion CompressPath


        #region Process Handling

        private void RunProcess( Process process ) {
            _ = process.Start( );
            process.BeginErrorReadLine( );
            process.BeginOutputReadLine( );
            process.WaitForExit( );
            FailOnNonZeroExitCodeOrException( process.ExitCode );
        }

        private Process Create7ZProcess(
            string arguments,
            string dependencyPath,
            string workingDirectory
        ) {
            using Activity? activity = s_source.StartActivity( "Create7ZProcess" )?.Start( );

            // Create Process
            Process process = new( ) {
                StartInfo = new( ) {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = dependencyPath,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = arguments
                }
            };
            process.ErrorDataReceived += ReceiveErrorData;

            process.OutputDataReceived += new DataReceivedEventHandler(
                ( sender, stdOut ) => {
                    if (string.IsNullOrWhiteSpace( stdOut.Data ) == false) {
                        _log?.LogInformation( "{string}", stdOut.Data );
                    }
                }
            );

            activity?.Stop( );
            return process;
        }

        private void ReceiveErrorData( object sender, DataReceivedEventArgs e ) {
            if (string.IsNullOrWhiteSpace( e?.Data ) == false) {
                _log?.LogCritical( "Compression Failure - Error: {string}", e.Data );
                if (sender is Process zipProcess && zipProcess.HasExited == false) {
                    zipProcess.Kill( );
                }
                _exceptions.Add( new FailedToZipException( e.Data ) );
            }
        }

        private void FailOnNonZeroExitCodeOrException( int exitCode ) {
            using Activity? activity = s_source.StartActivity( "FailOnNonZeroExitCodeOrException" )?.Start( );
            FailOnExceptions( );
            FailOnNonZeroExitCode( exitCode );
            activity?.Stop( );
        }

        private void FailOnExceptions( ) {
            if (_exceptions.Count > 0) {
                FailedToZipException[] exc = _exceptions.ToArray( );
                _exceptions.Clear( );
                _ = _semaphore.Release( );
                throw new AggregateException( exc );
            }
        }

        private void FailOnNonZeroExitCode( int exitCode ) {
            if (exitCode != 0) {
                string errorCodeDef = GetExitCodeDefinition( exitCode );
                string failedToZipExp = "Received non-zero exitcode from 7Zip. " +
                                        $"ExitCode: {exitCode} - {errorCodeDef}";
                _log?.LogCritical( "{string}", failedToZipExp );
                _ = _semaphore.Release( );
                throw new FailedToZipException( failedToZipExp );
            }
        }

        private static string GetExitCodeDefinition( int exitCode ) =>
            exitCode switch {
                1 => "Warning (Non fatal error(s)).",
                2 => "Fatal error",
                7 => "Command line error",
                8 => "Not enough memory for operation",
                255 => "User stopped the process",
                _ => "Unknown errorcode. Refer to 7-zip documentation for more details."
            };

        #endregion Process Handling

    }
}
