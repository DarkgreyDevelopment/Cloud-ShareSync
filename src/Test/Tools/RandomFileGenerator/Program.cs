using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace RandomFileGenerator {
    internal class Program {

        public const decimal OneKiB = 1024; // kibibyte
        public const decimal OneMiB = OneKiB * 1024; // mebibyte
        public const decimal OneGiB = OneMiB * 1024; // gibibyte
        public const decimal OneTiB = OneGiB * 1024; // tebibyte

        private static readonly RandomNumberGenerator s_random = RandomNumberGenerator.Create( );
        private static readonly Dictionary<string, int> s_fileSizeDictionary = new( );
        private static readonly Regex s_tb = new( @"^(?'digits'\d*\.?\d+)(?'sizeTrailer'\s?[tT][iI]?[bB])$", RegexOptions.Compiled );
        private static readonly Regex s_gb = new( @"^(?'digits'\d*\.?\d+)(?'sizeTrailer'\s?[gG][iI]?[bB])$", RegexOptions.Compiled );
        private static readonly Regex s_mb = new( @"^(?'digits'\d*\.?\d+)(?'sizeTrailer'\s?[mM][iI]?[bB])$", RegexOptions.Compiled );
        private static readonly Regex s_ext = new( @"^\.\w+$", RegexOptions.Compiled );

        private static DirectoryInfo WorkingDirectory { get; set; } = new( AppContext.BaseDirectory );
        private static string s_fileExtension = ".test";

        public static async Task Main( string[] args ) {
            foreach (string arg in args) {
                long fileSizeInBytes = ParseArgument( arg );
                if (fileSizeInBytes != 0) {
                    string fileName = GetFileName( fileSizeInBytes );
                    using FileStream fs = GetFileStream( fileName );
                    await WriteFile( fs, fileSizeInBytes );
                }
            }
        }

        private static long ParseArgument( string arg ) {
            if (Directory.Exists( arg )) {
                WorkingDirectory = new( arg );
                return 0;
            } else {
                Match matchTB = s_tb.Match( arg );
                Match matchGB = s_gb.Match( arg );
                Match matchMB = s_mb.Match( arg );

                decimal result;
                switch (true) {
                    case true when matchTB.Success && decimal.TryParse( matchTB.Groups["digits"].Value, out decimal tbVal ):
                        result = tbVal * OneTiB;
                        break;
                    case true when matchGB.Success && decimal.TryParse( matchGB.Groups["digits"].Value, out decimal gbVal ):
                        result = gbVal * OneGiB;
                        break;
                    case true when matchMB.Success && decimal.TryParse( matchMB.Groups["digits"].Value, out decimal mbVal ):
                        result = mbVal * OneMiB;
                        break;
                    case true when decimal.TryParse( arg, out decimal val ):
                        result = val * OneMiB;
                        break;
                    default:
                        if (s_ext.Match( arg ).Success) {
                            s_fileExtension = arg;
                        } else {
                            Console.WriteLine( $"Unable to parse the value of {arg}." );
                        }
                        return 0;
                }
                return (long)Math.Round( result, 0, MidpointRounding.AwayFromZero );
            }
        }

        private static string GetFileName( long size ) {
            string result;
            decimal value;
            string textValue;
            if (size >= OneTiB) {
                value = size / OneTiB;
                textValue = "TiB";
            } else if (size >= OneGiB) {
                value = size / OneGiB;
                textValue = "GiB";
            } else if (size >= OneMiB) {
                value = size / OneMiB;
                textValue = "MiB";
            } else if (size >= OneKiB) {
                value = size / OneKiB;
                textValue = "KiB";
            } else {
                value = size;
                textValue = "B";
            }

            result = (
                (value / 1 == 0) ?
                    Math.Round( value, 0, MidpointRounding.AwayFromZero ).ToString( ) :
                    Math.Round( value, 2, MidpointRounding.AwayFromZero ).ToString( )
            ) + textValue;

            if (s_fileSizeDictionary.ContainsKey( result )) {
                s_fileSizeDictionary[result]++;
                result += "-" + s_fileSizeDictionary[result];
            } else {
                s_fileSizeDictionary.Add( result, 1 );
            }
            return result + s_fileExtension;
        }

        private static async Task WriteFile( FileStream fs, long bytes ) {
            Console.WriteLine( $"Creating new file with a size of {bytes} bytes at '{fs.Name}'." );
            long count = 0;
            while (count < bytes) {
                if ((count + OneMiB) > bytes) {
                    int remainder = (int)(bytes - count);
                    await fs.WriteAsync( GetRandomData( new byte[remainder] ) );
                    count += remainder;
                } else {
                    await fs.WriteAsync( GetOneMiBRandomData( ) );
                    count += (long)OneMiB;
                }
            }
            Console.WriteLine( $"Successfully created a {count} byte file at '{fs.Name}'." );
        }

        private static byte[] GetRandomData( byte[] data ) {
            s_random.GetBytes( data );
            return data;
        }

        private static byte[] GetOneMiBRandomData( ) {
            byte[] data = new byte[(int)OneMiB];
            return GetRandomData( data );
        }

        private static FileStream GetFileStream( string encryptionFileName ) {
            string newFile = Path.Join( WorkingDirectory.FullName, encryptionFileName );
            return new FileStream(
                newFile,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                3145728, // 3mb buffer.
                FileOptions.Asynchronous
            );
        }

    }
}
