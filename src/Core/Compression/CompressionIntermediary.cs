using System.Diagnostics;
using Cloud_ShareSync.Core.Compression.Interfaces;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Compression {

    /// <summary>
    /// This class is used to compress/decompress files.
    /// Requires 7zip.
    /// </summary>
    internal class CompressionIntermediary : ICompression {

        private static readonly ActivitySource s_source = new( "CompressionInterface" );
        private readonly FileInfo _dependencyPath;
        private readonly ILogger? _log;
        private readonly SemaphoreSlim _semaphore = new( 0, 1 );

        private readonly List<FailedToZipException> _exceptions = new( );

        /// <summary>
        /// Pass in the 7zip dependency path via the <paramref name="config"/>.
        /// Optionally set <paramref name="log"/> to enable logging.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="log"></param>
        public CompressionIntermediary( CompressionConfig config, ILogger? log = null ) {
            _log = log;
            _dependencyPath = new( config.DependencyPath );
            SystemMemoryChecker.Update( );
            _ = _semaphore.Release( 1 );
        }


        #region DecompressPath

        /// <summary>
        /// ICompression interface method to decompress 7z files using the <see cref="CompressionIntermediary"/>.
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="decompressedPath"></param>
        /// <param name="password"></param>
        /// <returns>An enumeration of the decompressed FileSystemInfo objects</returns>
        public async Task<IEnumerable<FileSystemInfo>> DecompressPath(
            FileInfo inputPath,
            DirectoryInfo decompressedPath,
            string? password
        ) => await DecompressPath( inputPath, decompressedPath, _dependencyPath, password );

        internal async Task<IEnumerable<FileSystemInfo>> DecompressPath(
            FileInfo inputPath,
            DirectoryInfo decompressionDir,
            FileInfo dependencyPath,
            string? password = null
        ) {
            using Activity? activity = s_source.StartActivity( "DecompressPath" )?.Start( );
            await _semaphore.WaitAsync( );

            IEnumerable<string> existingItems = Enumerable.Empty<string>( );
            try {
                existingItems = Directory.EnumerateFileSystemEntries( dependencyPath.FullName );
            } catch { }

            _log?.LogInformation(
                "Extracting '{string}' into '{string}'.",
                inputPath.FullName,
                decompressionDir.FullName
            );
            SystemMemoryChecker.Update( );
            Process process = DecompressProcess(
                inputPath.FullName,
                decompressionDir.FullName,
                dependencyPath.FullName,
                decompressionDir.FullName,
                password
            );
            _ = process.Start( );
            process.BeginErrorReadLine( );
            process.BeginOutputReadLine( );
            process.WaitForExit( );
            FailOnNonZeroExitCodeOrException( process.ExitCode );
            _ = _semaphore.Release( );

            IEnumerable<string> allItems = Directory.EnumerateFileSystemEntries( dependencyPath.FullName );

            IEnumerable<string> decompressedItems = allItems.Where( e => existingItems.Contains( e ) == false );

            List<FileSystemInfo> result = new( );
            foreach (string item in decompressedItems) {
                if (File.Exists( item )) {
                    result.Add( new FileInfo( item ) );
                } else {
                    result.Add( new DirectoryInfo( item ) );
                }
            }

            _log?.LogInformation(
                "Successfully extracted '{string}'. \n" +
                "Extracted Item Count: '{string}'. \n",
                inputPath.FullName,
                decompressedItems.Count( )
            );
            activity?.Stop( );
            return result;
        }

        #endregion DecompressPath


        #region CompressPath

        /// <summary>
        /// ICompression interface method to compress files using 7zip via the <see cref="CompressionIntermediary"/>.
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

            _log?.LogInformation(
                "Compressing File '{string}' into '{string}'.",
                inputPath.FullName,
                compressedPath.FullName
            );
            SystemMemoryChecker.Update( );
            Process process = CompressProcess(
                inputPath.FullName,
                compressedPath.FullName,
                dependencyPath.FullName,
                compressedPath.Directory?.FullName ?? Path.GetTempPath( ),
                password
            );
            _ = process.Start( );
            process.BeginErrorReadLine( );
            process.BeginOutputReadLine( );
            process.WaitForExit( );
            FailOnNonZeroExitCodeOrException( process.ExitCode );
            _ = _semaphore.Release( );

            _log?.LogInformation(
                "Successfully compressed '{string}'. \n" +
                "Compressed FilePath: '{string}'. \n" +
                "Compressed FileSize: {long}.",
                inputPath.FullName,
                compressedPath.FullName,
                compressedPath.Length
            );
            activity?.Stop( );
            return compressedPath;
        }

        #endregion CompressPath


        #region PrivateMethods

        private Process DecompressProcess(
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

        private Process CompressProcess(
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
            if (_exceptions.Count > 0) {
                FailedToZipException[] exc = _exceptions.ToArray( );
                _exceptions.Clear( );
                _ = _semaphore.Release( );
                activity?.Stop( );
                throw new AggregateException( exc );
            }

            if (exitCode != 0) {
                string errorCodeDef = exitCode switch {
                    1 => "Warning (Non fatal error(s)).",
                    2 => "Fatal error",
                    7 => "Command line error",
                    8 => "Not enough memory for operation",
                    255 => "User stopped the process",
                    _ => "Unknown errorcode. Refer to 7-zip documentation for more details."
                };
                string failedToZipExp = $"Received non-zero exitcode from 7Zip. ExitCode: {exitCode} - {errorCodeDef}";
                _log?.LogCritical( "{string}", failedToZipExp );
                _ = _semaphore.Release( );
                activity?.Stop( );
                throw new FailedToZipException( failedToZipExp );
            }
            activity?.Stop( );
        }

        #endregion PrivateMethods

    }
}
