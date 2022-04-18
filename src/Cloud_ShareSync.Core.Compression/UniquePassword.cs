using System.Diagnostics;
using System.Security.Cryptography;

namespace Cloud_ShareSync.Core.Compression {
    public static class UniquePassword {

        #region Fields

        private static readonly ActivitySource s_source = new( "UniquePassword" );
        public static char[] UpperCase { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray( );
        public static char[] LowerCase { get; } = "abcdefghijklmnopqrstuvwxyz".ToCharArray( );
        public static char[] Numbers { get; } = "0123456789".ToCharArray( );
        public static char[] Symbols { get; } = { // Spaced out so its easier to see all the symbols.
            '\'', '!', '#', '$', '%', '&', ')', '(', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@',
            ']', '[', '^', '_', '|', '}', '{', '~', 'Ç', 'ü', 'é', 'â', 'ä', 'à', 'å', 'ç', 'ê', 'ë', 'è', 'ï', 'î',
            'ì', 'Ä', 'Å', 'É', 'æ', 'Æ', 'ô', 'ö', 'ò', 'û', 'ù', 'ÿ', 'Ö', 'Ü', 'ø', '£', 'Ø', '×', 'ƒ', 'á', 'í',
            'ó', 'ú', 'ñ', 'Ñ', 'ª', 'º', '¿', '®', '¬', '½', '¼', '¡', '┤', 'Á', 'Â', 'À', '©', '╣', '║', '╗', '╝',
            '¢', '¥', '┐', '└', '┴', '┬', '├', '─', '┼', 'ã', 'Ã', '╚', '╔', '╩', '╦', '╠', '═', '╬', '¤', 'ð', 'Ð',
            'Ê', 'Ë', 'È', 'ı', 'Í', 'Î', 'Ï', '┘', '┌', '¦', 'Ì', 'Ó', 'ß', 'Ô', 'Ò', 'õ', 'Õ', 'µ', 'þ', 'Þ', 'Ú',
            'Û', 'Ù', 'ý', 'Ý', '¬', '±', '‗', '¾', '¶', '§', '÷', '°', '¹', '³', '²'
        };
        public static char[] AllChars { get; } = Array.Empty<char>( );
        public static int AllCharsCount { get; } = AllChars.Length;

        #endregion Fields

        static UniquePassword( ) { AllChars = GetAllChars( ); }

        public static string Create( int length = 100 ) {
            using Activity? activity = s_source.StartActivity( "Create" )?.Start( );

            if (length <= 0) {
                throw new ArgumentOutOfRangeException( nameof( length ), "Length must be greater than 0." );
            }

            char[] output = new char[length];
            for (int x = 0; x < length; x++) {
                output[x] = AllChars[RandomNumberGenerator.GetInt32( AllCharsCount )];
            }

            activity?.Stop( );
            return ValidateComplexity( output );
        }

        #region HelperMethods
        private static string ValidateComplexity( char[] output ) {
            using Activity? activity = s_source.StartActivity( "ValidateComplexity" )?.Start( );
            List<int> positions = new( );
            char[] validOutput = output;

            ValidateReturn result = TestInitialComplexity( output, positions );

            validOutput = ReplaceMissing( result.HasUpper, validOutput, UpperCase, positions );
            validOutput = ReplaceMissing( result.HasLower, validOutput, LowerCase, positions );
            validOutput = ReplaceMissing( result.HasSymbol, validOutput, Symbols, positions );
            validOutput = ReplaceMissing( result.HasNumber, validOutput, Numbers, positions );

            activity?.Stop( );
            return new string( validOutput );
        }

        private static ValidateReturn TestInitialComplexity(
            char[] output,
            List<int> positions
        ) {
            ValidateReturn result = new( );
            int count = 0;
            foreach (char c in output) {
                if (ValidateReturn.IsComplex( result )) { break; }

                result.HasUpper = TestHasUpperCase( c, result.HasUpper, positions, count );
                result.HasLower = TestHasLowerCase( c, result.HasUpper, positions, count );
                result.HasSymbol = TestHasSymbol( c, result.HasSymbol, positions, count );
                result.HasNumber = TestHasNumber( c, result.HasNumber, positions, count );
                count++;
            }
            return result;
        }

        private static bool TestHasUpperCase( char c, bool hasUpper, List<int> positions, int count ) {
            if (hasUpper == false && UpperCase.Contains( c )) {
                positions.Add( count );
                return true;
            } else {
                return hasUpper;
            }
        }

        private static bool TestHasLowerCase( char c, bool hasLower, List<int> positions, int count ) {
            if (hasLower == false && LowerCase.Contains( c )) {
                positions.Add( count );
                return true;
            } else {
                return hasLower;
            }
        }

        private static bool TestHasSymbol( char c, bool hasSymbol, List<int> positions, int count ) {
            if (hasSymbol == false && Symbols.Contains( c )) {
                positions.Add( count );
                return true;
            } else {
                return hasSymbol;
            }
        }

        private static bool TestHasNumber( char c, bool hasNumber, List<int> positions, int count ) {
            if (hasNumber == false && Numbers.Contains( c )) {
                positions.Add( count );
                return true;
            } else {
                return hasNumber;
            }
        }

        private static char[] ReplaceMissing(
            bool shouldReplace,
            char[] output,
            char[] characterSet,
            List<int> positions
        ) {
            using Activity? activity = s_source.StartActivity( "ReplaceMissing" )?.Start( );

            if (shouldReplace) {
                int replacePosition;
                bool resolved = false;
                do {
                    replacePosition = RandomNumberGenerator.GetInt32( output.Length );
                    if (positions.Contains( replacePosition ) == false) {
                        output[replacePosition] = characterSet[RandomNumberGenerator.GetInt32( characterSet.Length )];
                        positions.Add( replacePosition );
                        resolved = true;
                    }
                } while (resolved == false);
            }

            activity?.Stop( );
            return output;
        }

        private static char[] GetAllChars( ) {
            using Activity? activity = s_source.StartActivity( "Create" )?.Start( );

            List<char> charlist = new( );
            charlist.AddRange( UpperCase );
            charlist.AddRange( LowerCase );
            charlist.AddRange( Symbols );
            charlist.AddRange( Numbers );

            activity?.Stop( );
            return charlist.ToArray( );
        }
        #endregion HelperMethods

        private class ValidateReturn {
            public bool HasUpper { get; set; }
            public bool HasLower { get; set; }
            public bool HasSymbol { get; set; }
            public bool HasNumber { get; set; }

            public static bool IsComplex( ValidateReturn result ) {
                return result.HasUpper & result.HasLower & result.HasSymbol & result.HasNumber;
            }
        }
    }
}
