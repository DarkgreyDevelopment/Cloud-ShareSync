using System.Diagnostics;
using System.Security.Cryptography;

namespace Cloud_ShareSync.Core.Compression {
    public static class UniquePassword {

        #region Fields
        private static readonly ActivitySource s_source  = new( "UniquePassword" );
        public static  readonly char[]         UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray( );
        public static  readonly char[]         LowerCase = "abcdefghijklmnopqrstuvwxyz".ToCharArray( );
        public static  readonly char[]         Numbers   = "0123456789".ToCharArray( );
        private static readonly char[]         s_symbols = { // Spaced out so its easier to see all the symbols.
            '\'', '!', '#', '$', '%', '&', ')', '(', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@',
            ']', '[', '^', '_', '|', '}', '{', '~', 'Ç', 'ü', 'é', 'â', 'ä', 'à', 'å', 'ç', 'ê', 'ë', 'è', 'ï', 'î',
            'ì', 'Ä', 'Å', 'É', 'æ', 'Æ', 'ô', 'ö', 'ò', 'û', 'ù', 'ÿ', 'Ö', 'Ü', 'ø', '£', 'Ø', '×', 'ƒ', 'á', 'í',
            'ó', 'ú', 'ñ', 'Ñ', 'ª', 'º', '¿', '®', '¬', '½', '¼', '¡', '┤', 'Á', 'Â', 'À', '©', '╣', '║', '╗', '╝',
            '¢', '¥', '┐', '└', '┴', '┬', '├', '─', '┼', 'ã', 'Ã', '╚', '╔', '╩', '╦', '╠', '═', '╬', '¤', 'ð', 'Ð',
            'Ê', 'Ë', 'È', 'ı', 'Í', 'Î', 'Ï', '┘', '┌', '¦', 'Ì', 'Ó', 'ß', 'Ô', 'Ò', 'õ', 'Õ', 'µ', 'þ', 'Þ', 'Ú',
            'Û', 'Ù', 'ý', 'Ý', '¬', '±', '‗', '¾', '¶', '§', '÷', '°', '¹', '³', '²'
        };
        private static readonly List<int>      s_positions = new( );

        public static char[] AllChars       { get { return GetAllChars( ); } }

        private static int   AllCharsLength { get; } = AllChars.Length;

        #endregion Fields

        public static string Create( int length = 100 ) {
            using Activity? activity = s_source.StartActivity( "Create" );
            activity?.Start( );

            char[] output = new char[length];
            for (int x = 0; x < length; x++) {
                output[x] = AllChars[RandomNumberGenerator.GetInt32( AllCharsLength )];
            }

            activity?.Stop( );
            return ValidateComplexity( output );
        }


        #region HelperMethods
        private static string ValidateComplexity( char[] output ) {
            using Activity? activity = s_source.StartActivity( "ValidateComplexity" )?.Start( );

            bool hasUpper  = false;
            bool hasLower  = false;
            bool hasSymbol = false;
            bool hasNumber = false;

            char[] validOutput = output;
            int count = 0;
            foreach (char c in output) {
                if (hasUpper && hasLower && hasSymbol && hasNumber) { break; }
                if ((hasUpper  == false) && UpperCase.Contains( c )) { hasUpper  = true; s_positions.Add( count ); }
                if ((hasLower  == false) && LowerCase.Contains( c )) { hasLower  = true; s_positions.Add( count ); }
                if ((hasSymbol == false) && s_symbols.Contains( c ))   { hasSymbol = true; s_positions.Add( count ); }
                if ((hasNumber == false) && Numbers.Contains( c ))   { hasNumber = true; s_positions.Add( count ); }
                count++;
            }

            if (hasUpper  == false) { validOutput = ReplaceMissing( validOutput, UpperCase ); }
            if (hasLower  == false) { validOutput = ReplaceMissing( validOutput, LowerCase ); }
            if (hasSymbol == false) { validOutput = ReplaceMissing( validOutput, s_symbols ); }
            if (hasNumber == false) { validOutput = ReplaceMissing( validOutput, Numbers ); }

            activity?.Stop( );
            return new string( validOutput );
        }

        private static char[] ReplaceMissing(
            char[] output,
            char[] characterSet
        ) {
            using Activity? activity = s_source.StartActivity( "ReplaceMissing" )?.Start( );

            int replacePosition;
            bool resolved = false;
            do {
                replacePosition = RandomNumberGenerator.GetInt32( output.Length );
                if (!s_positions.Contains( replacePosition )) {
                    output[replacePosition] = characterSet[RandomNumberGenerator.GetInt32( characterSet.Length )];
                    s_positions.Add( replacePosition );
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
            charlist.AddRange( s_symbols );
            charlist.AddRange( Numbers );

            activity?.Stop( );
            return charlist.ToArray( );
        }
        #endregion HelperMethods
    }
}
