using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using System;

namespace Funky.Tokens.Literal{
    class TLiteralList : TLiteral{

        private static Regex LEFT_BRACKET = new Regex(@"\[");
        private static Regex RIGHT_BRACKET = new Regex(@"\]");
        private static Regex COMMA = new Regex(@",");

        new public static TLiteralList Claim(StringClaimer claimer){
            Claim c = claimer.Claim(LEFT_BRACKET);
            if(!c.success){
                return null;
            }



            return null;
        }

        override public Var Parse(Scope scope){
            return null;
        }
    }
}