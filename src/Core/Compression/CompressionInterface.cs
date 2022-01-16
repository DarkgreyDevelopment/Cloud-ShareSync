using System.Diagnostics;
using Cloud_ShareSync.Core.Configuration.Types.Features;
using Microsoft.Extensions.Logging;

namespace Cloud_ShareSync.Core.Compression {
    public static class CompressionInterface {

        private static readonly ActivitySource s_source                 = new( "CompressionInterface" );
        private static          string         s_compressionArguments   = "";
        private static          string         s_decompressionArguments = "";
        private static          FileInfo?      s_dependencyPath;
        private static          ILogger?       s_log;

#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static DirectoryInfo? WorkingDirectory;
        public static string         InterimZipname = "InterimCompressionItem.7z";
#pragma warning restore CA2211 // Non-constant fields should not be visible

        public static void Initialize( CompressionConfig config, ILogger? log = null ) {
            s_log                    = log;
            s_dependencyPath         = new( config.DependencyPath );
            WorkingDirectory         = new( config.InterimZipPath );
            s_compressionArguments   = config.DeCompressionCmdlineArgs;
            s_decompressionArguments = config.CompressionCmdlineArgs;
            InterimZipname           = config.InterimZipName.EndsWith( ".7z" ) ?
                                        config.InterimZipName :
                                        config.InterimZipName + ".7z";
        }

        public static void DecompressPath( ) {
            Console.WriteLine( $"{s_decompressionArguments}" );
            throw new NotImplementedException( );
        }

        public static FileInfo CompressPath(
            FileInfo path,
            string?  password = null
        ) {
            return s_dependencyPath == null || WorkingDirectory == null
                ? throw new InvalidOperationException(
                    "DependencyPath & WorkingDirectory are required. " +
                    "Inititalize CompressionInterface first or use alternate CompressPath method."
                )
                : CompressPath( path, s_dependencyPath, WorkingDirectory, password, s_compressionArguments);
        }

        public static FileInfo CompressPath(
            FileInfo      path,
            FileInfo      dependencyPath,
            DirectoryInfo workingDirectory,
            string?       password  = null,
            string?       arguments = null
        ) {
            using Activity? activity = s_source.StartActivity( "CompressPath" )?.Start( );

            s_log?.LogInformation( "Compressing File '{string}'.", path.FullName );
            long memUsage = GC.GetTotalMemory( true );
            s_log?.LogDebug( "Current Memory Usage: {long}.", memUsage );

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

            activity?.Stop( );
            return new FileInfo( Path.Join( workingDirectory.FullName, InterimZipname ) );
        }

        private static string GetInterimZipPath( DirectoryInfo workingDirectory ) {
            using Activity? activity = s_source.StartActivity( "GetInterimZipPath" )?.Start( );

            string interimZipPath = Path.Join( workingDirectory.FullName, InterimZipname );
            if (File.Exists( interimZipPath )) {
                s_log?.LogInformation( "Deleting '{string}' left over from previous run.", interimZipPath );
                File.Delete( interimZipPath );
            }

            activity?.Stop( );
            return interimZipPath;
        }

        private static Process Create7zProcess(
            string        interimZipPath,
            FileInfo      zipPath,
            FileInfo      dependencyPath,
            DirectoryInfo workingDirectory,
            string?       password  = null,
            string?       arguments = null
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
                    WindowStyle            = ProcessWindowStyle.Hidden,
                    FileName               = $"{dependencyPath.FullName}",
                    WorkingDirectory       = workingDirectory.FullName,
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    Arguments              = arguments
                }
            };
            process.ErrorDataReceived += new DataReceivedEventHandler( ( sender, e ) => {
                    if (!string.IsNullOrWhiteSpace( e.Data )) {
                        s_log?.LogCritical( "{string}", e.Data );
                        activity?.Stop( );
                        throw new FailedToZipException( e.Data );
                    }
                }
            );

            process.OutputDataReceived += new DataReceivedEventHandler( ( sender, stdOut ) => {
                    if (!string.IsNullOrWhiteSpace( stdOut.Data )) { s_log?.LogInformation( "{string}", stdOut.Data ); }
                }
            );

            activity?.Stop( );
            return process;
        }

        private static void FailOnNonZeroExitCode( int exitCode ) {
            if (exitCode != 0) {
                string failedToZipExp = $"Received non-zero exitcode from 7Zip. ExitCode: {exitCode}";
                s_log?.LogCritical( "{string}", failedToZipExp );
                throw new FailedToZipException( failedToZipExp );
            }
        }

    }
}
