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
            FileSystemInfo path,
            FileInfo compressedPath,
            string? password = null
        ) => await CompressPath( path, compressedPath, _dependencyPath, password );

        public async Task<FileInfo> CompressPath(
            FileSystemInfo path,
            FileInfo compressedPath,
            FileInfo dependencyPath,
            string? password = null
        ) {
            using Activity? activity = s_source.StartActivity( "CompressPath" )?.Start( );
            await _semaphore.WaitAsync( );

            _log?.LogInformation(
                "Compressing File '{string}' into '{string}'.",
                path.FullName,
                compressedPath.FullName
            );
            SystemMemoryChecker.Update( );

            Process process = Create7zProcess(
                compressedPath.FullName,
                path,
                dependencyPath,
                compressedPath.Directory ?? new( Path.GetTempPath( ) ),
                password
            );
            process.Start( );
            process.BeginErrorReadLine( );
            process.BeginOutputReadLine( );
            process.WaitForExit( );
            _semaphore.Release( );
            FailOnNonZeroExitCode( process.ExitCode );

            _log?.LogInformation(
                "Successfully compressed '{string}'. \n" +
                "Compressed FilePath: '{string}'. \n" +
                "Compressed FileSize: {long}.",
                path.FullName,
                compressedPath.FullName,
                compressedPath.Length
            );
            activity?.Stop( );
            return compressedPath;
        }

        #endregion CompressPath


        #region PrivateMethods

        private Process Create7zProcess(
            string interimZipPath,
            FileSystemInfo zipPath,
            FileInfo dependencyPath,
            DirectoryInfo workingDirectory,
            string? password = null
        ) {
            using Activity? activity = s_source.StartActivity( "GetInterimZipPath" )?.Start( );

            // 7z Cmdline Arguments - Works on both linux + windows.
            string arguments = $"a {interimZipPath} -mfb=257 -mx=9 -mhe=on -mmt=on ";

            arguments += string.IsNullOrWhiteSpace( password ) ?
                                $"-- \"{zipPath.FullName}\"" :
                                $"-p\"{password}\" -- \"{zipPath.FullName}\"";

            // Create Process
            Process process = new( ) {
                StartInfo = new( ) {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = $"{dependencyPath.FullName}",
                    WorkingDirectory = workingDirectory.FullName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = arguments
                }
            };
            process.ErrorDataReceived += new DataReceivedEventHandler( ( sender, e ) => {
                if (!string.IsNullOrWhiteSpace( e.Data )) {
                    _log?.LogCritical( "Zip Failure - {string}", e.Data );
                    activity?.Stop( );
                    throw new FailedToZipException( e.Data );
                }
            }
            );

            process.OutputDataReceived += new DataReceivedEventHandler(
                ( sender, stdOut ) => {
                    if (!string.IsNullOrWhiteSpace( stdOut.Data )) {
                        _log?.LogInformation( "{string}", stdOut.Data );
                    }
                }
            );

            activity?.Stop( );
            return process;
        }

        private void FailOnNonZeroExitCode( int exitCode ) {
            if (exitCode != 0) {
                string failedToZipExp = $"Received non-zero exitcode from 7Zip. ExitCode: {exitCode}";
                _log?.LogCritical( "{string}", failedToZipExp );
                throw new FailedToZipException( failedToZipExp );
            }
        }

        #endregion PrivateMethods

    }
}
