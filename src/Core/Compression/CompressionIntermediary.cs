using System.Diagnostics;
using Cloud_ShareSync.Core.Compression.Interfaces;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Compression {
    public class CompressionIntermediary : ICompression {

        private static readonly ActivitySource s_source = new( "CompressionInterface" );
        private readonly FileInfo _dependencyPath;
        private readonly ILogger? _log;
        private readonly SemaphoreSlim _semaphore = new( 0, 1 );

        private readonly List<FailedToZipException> _exceptions = new( );

        public CompressionIntermediary( CompressionConfig config, ILogger? log = null ) {
            _log = log;
            _dependencyPath = new( config.DependencyPath );
            SystemMemoryChecker.Update( );
            _semaphore.Release( 1 );
        }


        #region DecompressPath

        public async Task<FileSystemInfo> DecompressPath(
            FileInfo path,
            FileInfo decompressedPath,
            string? password
        ) {
            await _semaphore.WaitAsync( );
            _semaphore.Release( );
            throw new NotImplementedException( );
        }

        #endregion DecompressPath


        #region CompressPath

        public async Task<FileInfo> CompressPath(
            FileSystemInfo inputPath,
            FileInfo compressedPath,
            string? password = null
        ) => await CompressPath( inputPath, compressedPath, _dependencyPath, password );

        public async Task<FileInfo> CompressPath(
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
            Process process = Create7zProcess(
                inputPath.FullName,
                compressedPath.FullName,
                dependencyPath.FullName,
                compressedPath.Directory?.FullName ?? Path.GetTempPath( ),
                password
            );
            process.Start( );
            process.BeginErrorReadLine( );
            process.BeginOutputReadLine( );
            process.WaitForExit( );
            FailOnNonZeroExitCodeOrException( process.ExitCode );
            _semaphore.Release( );

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

        private Process Create7zProcess(
            string inputPath,
            string outputPath,
            string dependencyPath,
            string workingDirectory,
            string? password = null
        ) {
            using Activity? activity = s_source.StartActivity( "GetInterimZipPath" )?.Start( );

            // 7z Cmdline Arguments - Works on both linux + windows.
            string arguments = $"a \"{outputPath}\" -mfb=257 -mx=9 -mhe=on -mmt=on ";

            arguments += string.IsNullOrWhiteSpace( password ) ?
                                $"-- \"{inputPath}\"" :
                                $"-p\"{password}\" -- \"{inputPath}\"";
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
            if (_exceptions.Count > 0) {
                FailedToZipException[] exc = _exceptions.ToArray( );
                _exceptions.Clear( );
                _semaphore.Release( );
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
                _semaphore.Release( );
                throw new FailedToZipException( failedToZipExp );
            }
        }

        #endregion PrivateMethods

    }
}
