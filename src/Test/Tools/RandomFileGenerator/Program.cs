using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace RandomFileGenerator {
    internal class Program {

        public const decimal oneKiB = 1024; // kibibyte
        public const decimal oneMiB = oneKiB * 1024; // mebibyte
        public const decimal oneGiB = oneMiB * 1024; // gibibyte
        public const decimal oneTiB = oneGiB * 1024; // tebibyte

        private static readonly RandomNumberGenerator random = RandomNumberGenerator.Create();
        private static readonly Dictionary<string, int> fileSizeDictionary = new();
        private static readonly Regex tb = new(@"^(?'digits'\d*\.?\d+)(?'sizeTrailer'\s?[tT][iI]?[bB])$", RegexOptions.Compiled);
        private static readonly Regex gb = new(@"^(?'digits'\d*\.?\d+)(?'sizeTrailer'\s?[gG][iI]?[bB])$", RegexOptions.Compiled);
        private static readonly Regex mb = new(@"^(?'digits'\d*\.?\d+)(?'sizeTrailer'\s?[mM][iI]?[bB])$", RegexOptions.Compiled);
        private static readonly Regex ext = new(@"^\.\w+$", RegexOptions.Compiled);

        private static DirectoryInfo WorkingDirectory { get; set; } = Directory.GetParent(Assembly.GetExecutingAssembly().Location) ?? new("");
        private static string FileExtension = ".test";

        public static async Task Main(string[] args) {
            foreach (var arg in args) {
                long FileSizeInBytes = ParseArgument(arg);
                if (FileSizeInBytes != 0) {
                    string fileName = GetFileName(FileSizeInBytes);
                    using FileStream fs = GetFileStream(fileName);
                    await WriteFile(fs, FileSizeInBytes);
                }
            }
        }

        private static long ParseArgument( string arg ) {
            if (Directory.Exists(arg)) {
                WorkingDirectory = new(arg);
                return 0;
            } else {
                Match matchTB = tb.Match(arg);
                Match matchGB = gb.Match(arg);
                Match matchMB = mb.Match(arg);

                decimal result;
                switch (true) {
                    case true when matchTB.Success && decimal.TryParse(matchTB.Groups["digits"].Value, out decimal tbVal):
                        result = tbVal * oneTiB;
                        break;
                    case true when matchGB.Success && decimal.TryParse(matchGB.Groups["digits"].Value, out decimal gbVal):
                        result = gbVal * oneGiB;
                        break;
                    case true when matchMB.Success && decimal.TryParse(matchMB.Groups["digits"].Value, out decimal mbVal):
                        result = mbVal * oneMiB;
                        break;
                    case true when decimal.TryParse(arg, out decimal val):
                        result = val * oneMiB;
                        break;
                    default:
                         if (ext.Match(arg).Success) {
                            FileExtension = arg;
                        } else { 
                            Console.WriteLine($"Unable to parse the value of {arg}.");
                        }
                        return 0;
                }
                return (long)Math.Round(result, 0, MidpointRounding.AwayFromZero);
            }
        }

        private static string GetFileName( long size ) {
            string result;
            decimal value;
            string textValue;
            if (size >= oneTiB){
                value = size / oneTiB;
                textValue = "TiB";
            } else if (size >= oneGiB) {
                value = size / oneGiB;
                textValue = "GiB";
            } else if (size >= oneMiB) {
                value = size / oneMiB;
                textValue = "MiB";
            } else if (size >= oneKiB) {
                value = size / oneKiB;
                textValue = "KiB";
            } else {
                value = size;
                textValue = "B";
            }
            if (value / 1 == 0) {
                result = Math.Round(value, 0, MidpointRounding.AwayFromZero).ToString() + textValue;
            } else {
                result = Math.Round(value, 2, MidpointRounding.AwayFromZero).ToString() + textValue;
            }
            if (fileSizeDictionary.ContainsKey(result)) {
                fileSizeDictionary[result]++;
                result += "-" + fileSizeDictionary[result];
            } else {
                fileSizeDictionary.Add(result, 1);
            }
            return result + FileExtension;
        }

        private static async Task WriteFile(FileStream fs, long bytes) {
            Console.WriteLine($"Creating new file with a size of {bytes} bytes at '{fs.Name}'.");
            long count = 0;
            while (count < bytes) {
                if ((count + oneMiB) > bytes) {
                    int remainder = (int)(bytes - count);
                    await fs.WriteAsync(GetRandomData(new byte[remainder]));
                    count += remainder;
                } else {
                    await fs.WriteAsync(GetOneMiBRandomData());
                    count += (long)oneMiB;
                }
            }
            Console.WriteLine($"Successfully created a {count} byte file at '{fs.Name}'.");
        }

        private static byte[] GetRandomData( byte[] data) {
            random.GetBytes(data);
            return data;
        }

        private static byte[] GetOneMiBRandomData() {
            byte[] data = new byte[(int)oneMiB];
            return GetRandomData(data);
        }

        private static FileStream GetFileStream(string encryptionFileName) {
            string newFile = Path.Join(WorkingDirectory.FullName, encryptionFileName);
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