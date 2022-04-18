using System.Text;

namespace Cloud_ShareSync.Configuration.ManagedActions {
    internal static class ConfigPathHandler {

        private static readonly string s_assemblyPath = AppContext.BaseDirectory;
        private static readonly string s_defaultConfig = Path.Join( s_assemblyPath, "appsettings.json" );
        private static readonly string s_altConfigInfo = Path.Join( s_assemblyPath, ".configpath" );

        public static string GetConfigPath( bool useDefaultOnErr ) {
            try {
                return GetConfigurationPath( );
            } catch {
                if (useDefaultOnErr) {
                    return s_defaultConfig;
                } else {
                    throw;
                }
            }
        }

        private static string GetConfigurationPath( ) {
            string? envConfig = Environment.GetEnvironmentVariable( "CLOUDSHARESYNC_CONFIGPATH" );
            string? altConfigPath = GetAltDefaultConfigPath( );

            return true switch {
                true when altConfigPath != null && File.Exists( altConfigPath ) => altConfigPath,
                true when File.Exists( s_defaultConfig ) => s_defaultConfig,
                true when envConfig != null && File.Exists( envConfig ) => envConfig,
                true when altConfigPath != null => altConfigPath,
                _ => throw new ApplicationException(
                    "\nMissing required configuration file. " +
                    "The configuration file path can be specified in one of three ways.\n" +
                    "  1. Pass the path to the configuration file via the --ConfigPath cmdline " +
                    "option. Using the --ConfigPath option will set a new default config location.\n" +
                    $"  2. Put the config file in the default config path '{s_defaultConfig}'. \n" +
                    "  3. Set the 'CLOUDSHARESYNC_CONFIGPATH' environment variable with a valid file path.\n" +
                    "You can also use the 'Configure' command to customize the config. " +
                    "See 'Cloud-ShareSync Configure -h' for more information." +
                    (altConfigPath != null ? $"\nSpecified ConfigPath '{altConfigPath}' does not exist.\n" : "\n")
                )
            };
        }


        public static void SetAltDefaultConfigPath( string path ) {
            Console.WriteLine( $"Setting default config path to '{path}'." );
            string base64path = Convert.ToBase64String( Encoding.UTF8.GetBytes( path ) );
            File.WriteAllText( s_altConfigInfo, base64path );
        }

        private static string? GetAltDefaultConfigPath( ) {
            if (File.Exists( s_altConfigInfo )) {
                string path = ReadAltDefaultConfigInfo( File.ReadAllText( s_altConfigInfo ) );
                if (File.Exists( path ) == false) {
                    Console.WriteLine(
                        $"Missing Alternate Default Config Path: {path}\n" +
                        $"The file path specified in '{s_altConfigInfo}' does not exist. " +
                        "This may lead to errors unless an alternate is specified via --ConfigPath."
                    );
                }
                return path;
            } else {
                return null;
            }
        }

        private static string ReadAltDefaultConfigInfo( string base64EncodedData ) =>
            Encoding.UTF8.GetString( Convert.FromBase64String( base64EncodedData ) );

    }
}
