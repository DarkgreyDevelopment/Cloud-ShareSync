using System.Diagnostics;
using Cloud_ShareSync.Core.Configuration.Types;
using Cloud_ShareSync.Core.SharedServices;
using Microsoft.Extensions.Logging;
using Cloud_ShareSync.Core.Compression.Interfaces;

namespace Cloud_ShareSync.Core.Compression {
    public class CompressionIntermediary : ICompression {

        private static readonly ActivitySource s_source = new( "CompressionInterface" );
        private readonly string _compressionArguments = "";
        private readonly string _decompressionArguments = "";
        private readonly FileInfo _dependencyPath;
        private readonly ILogger? _log;

        public DirectoryInfo _workingDirectory;
        public string _interimZipname = "InterimCompressionItem.7z";

        public CompressionIntermediary( CompressionConfig config, ILogger? log = null ) {
            _log = log;
            _dependencyPath = new( config.DependencyPath );
            _workingDirectory = new( config.InterimZipPath );
            _compressionArguments = config.CompressionCmdlineArgs;
            _decompressionArguments = config.DeCompressionCmdlineArgs;
            _interimZipname = config.InterimZipName;
            SystemMemoryChecker.Update( );
        }


        #region DecompressPath

        public FileSystemInfo DecompressPath( FileInfo path, string? password ) {
            _log?.LogInformation( "{string}", _decompressionArguments );
            throw new NotImplementedException( );
        }

        #endregion DecompressPath


        #region CompressPath

        public FileInfo CompressPath(
            FileSystemInfo path,
            string? password = null
        ) => CompressPath( path, _dependencyPath, _workingDirectory, password, _compressionArguments );

        public FileInfo CompressPath(
            FileSystemInfo path,
            FileInfo dependencyPath,
            DirectoryInfo workingDirectory,
            string? password = null,
            string? arguments = null
        ) {
            using Activity? activity = s_source.StartActivity( "CompressPath" )?.Start( );

            _log?.LogInformation( "Compressing File '{string}'.", path.FullName );
            SystemMemoryChecker.Update( );

            string interimZipPath = GetInterimZipPath( workingDirectory );
            Process process = Create7zProcess(
                interimZipPath,
                path,
                dependencyPath,
                workingDirectory,
                password,
                arguments
            );
            process.Start( );
            process.BeginErrorReadLine( );
            process.BeginOutputReadLine( );
            process.WaitForExit( );
            FailOnNonZeroExitCode( process.ExitCode );

            FileInfo result = new( Path.Join( workingDirectory.FullName, _interimZipname ) );

            _log?.LogInformation(
                "Successfully compressed '{string}'. \n" +
                "Compressed FilePath: '{string}'. \n" +
                "Compressed FileSize: {long}.",
                path.FullName,
                result.FullName,
                result.Length
            );
            activity?.Stop( );
            return result;
        }

        #endregion CompressPath


        #region PrivateMethods

        private string GetInterimZipPath( DirectoryInfo workingDirectory ) {
            using Activity? activity = s_source.StartActivity( "GetInterimZipPath" )?.Start( );

            string interimZipPath = Path.Join( workingDirectory.FullName, _interimZipname );
            if (File.Exists( interimZipPath )) {
                _log?.LogInformation( "Deleting '{string}' left over from previous run.", interimZipPath );
                File.Delete( interimZipPath );
            }

            activity?.Stop( );
            return interimZipPath;
        }

        private Process Create7zProcess(
            string interimZipPath,
            FileSystemInfo zipPath,
            FileInfo dependencyPath,
            DirectoryInfo workingDirectory,
            string? password = null,
            string? arguments = null
        ) {
            using Activity? activity = s_source.StartActivity( "GetInterimZipPath" )?.Start( );

            // 7z Cmdline Arguments - Works on both linux + windows.
            arguments = string.IsNullOrWhiteSpace( arguments ) ?
                                $"a {interimZipPath} -mfb=257 -mx=9 -mhe=on -mmt=on " :
                                $"a {interimZipPath} {arguments} ";

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
                    _log?.LogCritical( "{string}", e.Data );
                    activity?.Stop( );
                    throw new FailedToZipException( e.Data );
                }
            }
            );

            process.OutputDataReceived += new DataReceivedEventHandler( ( sender, stdOut ) => {
                if (!string.IsNullOrWhiteSpace( stdOut.Data )) { _log?.LogInformation( "{string}", stdOut.Data ); }
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
