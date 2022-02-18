using System.Diagnostics;
using System.Security.Cryptography;

namespace Cloud_ShareSync.Core.Compression {
    public class UniquePassword {

        #region Fields

        private static readonly ActivitySource s_source = new( "UniquePassword" );
        public static char[] UpperCase { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray( );
        public static char[] LowerCase { get; set; } = "abcdefghijklmnopqrstuvwxyz".ToCharArray( );
        public static char[] Numbers { get; set; } = "0123456789".ToCharArray( );
        public static char[] Symbols { get; set; } = { // Spaced out so its easier to see all the symbols.
            '\'', '!', '#', '$', '%', '&', ')', '(', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@',
            ']', '[', '^', '_', '|', '}', '{', '~', 'Ç', 'ü', 'é', 'â', 'ä', 'à', 'å', 'ç', 'ê', 'ë', 'è', 'ï', 'î',
            'ì', 'Ä', 'Å', 'É', 'æ', 'Æ', 'ô', 'ö', 'ò', 'û', 'ù', 'ÿ', 'Ö', 'Ü', 'ø', '£', 'Ø', '×', 'ƒ', 'á', 'í',
            'ó', 'ú', 'ñ', 'Ñ', 'ª', 'º', '¿', '®', '¬', '½', '¼', '¡', '┤', 'Á', 'Â', 'À', '©', '╣', '║', '╗', '╝',
            '¢', '¥', '┐', '└', '┴', '┬', '├', '─', '┼', 'ã', 'Ã', '╚', '╔', '╩', '╦', '╠', '═', '╬', '¤', 'ð', 'Ð',
            'Ê', 'Ë', 'È', 'ı', 'Í', 'Î', 'Ï', '┘', '┌', '¦', 'Ì', 'Ó', 'ß', 'Ô', 'Ò', 'õ', 'Õ', 'µ', 'þ', 'Þ', 'Ú',
            'Û', 'Ù', 'ý', 'Ý', '¬', '±', '‗', '¾', '¶', '§', '÷', '°', '¹', '³', '²'
        };
        private static char[] AllChars { get { return GetAllChars( ); } }
        private static int AllCharsCount { get; } = AllChars.Length;
        private List<int> _positions = new( );

        #endregion Fields

        public string Create( int length = 100 ) {
            using Activity? activity = s_source.StartActivity( "Create" );
            activity?.Start( );

            if (length <= 0) {
                throw new ArgumentOutOfRangeException( nameof( length ), "Length must be greater than 0." );
            }

            _positions = new( );

            char[] output = new char[length];
            for (int x = 0; x < length; x++) {
                output[x] = AllChars[RandomNumberGenerator.GetInt32( AllCharsCount )];
            }

            activity?.Stop( );
            return ValidateComplexity( output );
        }


        #region HelperMethods
        private string ValidateComplexity( char[] output ) {
            using Activity? activity = s_source.StartActivity( "ValidateComplexity" )?.Start( );

            bool hasUpper = false;
            bool hasLower = false;
            bool hasSymbol = false;
            bool hasNumber = false;

            char[] validOutput = output;
            int count = 0;
            foreach (char c in output) {
                if (hasUpper && hasLower && hasSymbol && hasNumber) { break; }
                if ((hasUpper == false) && UpperCase.Contains( c )) { hasUpper = true; _positions.Add( count ); }
                if ((hasLower == false) && LowerCase.Contains( c )) { hasLower = true; _positions.Add( count ); }
                if ((hasSymbol == false) && Symbols.Contains( c )) { hasSymbol = true; _positions.Add( count ); }
                if ((hasNumber == false) && Numbers.Contains( c )) { hasNumber = true; _positions.Add( count ); }
                count++;
            }

            if (hasUpper == false) { validOutput = ReplaceMissing( validOutput, UpperCase ); }
            if (hasLower == false) { validOutput = ReplaceMissing( validOutput, LowerCase ); }
            if (hasSymbol == false) { validOutput = ReplaceMissing( validOutput, Symbols ); }
            if (hasNumber == false) { validOutput = ReplaceMissing( validOutput, Numbers ); }

            activity?.Stop( );
            return new string( validOutput );
        }

        private char[] ReplaceMissing(
            char[] output,
            char[] characterSet
        ) {
            using Activity? activity = s_source.StartActivity( "ReplaceMissing" )?.Start( );

            int replacePosition;
            bool resolved = false;
            do {
                replacePosition = RandomNumberGenerator.GetInt32( output.Length );
                if (!_positions.Contains( replacePosition )) {
                    output[replacePosition] = characterSet[RandomNumberGenerator.GetInt32( characterSet.Length )];
                    _positions.Add( replacePosition );
                    resolved = true;
                }
            } while (!resolved);

            activity?.Stop( );
            return output;
        }

        private static char[] GetAllChars( ) {
            using Activity? activity = s_source.StartActivity( "Create" );
            activity?.Start( );

            List<char> charlist = new( );
            charlist.AddRange( UpperCase );
            charlist.AddRange( LowerCase );
            charlist.AddRange( Symbols );
            charlist.AddRange( Numbers );

            activity?.Stop( );
            return charlist.ToArray( );
        }
        #endregion HelperMethods
    }
}
