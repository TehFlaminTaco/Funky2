
using System.Collections.Generic;

namespace Funky.Tokens{
    abstract class Token{
        public string raw = "";

        public static Token Claim(StringClaimer claimer){
            return null;
        }
    }
}