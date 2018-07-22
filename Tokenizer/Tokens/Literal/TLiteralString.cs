using System.Text.RegularExpressions;
using System.Text;
using System;

namespace Funky.Tokens.Literal{

    class TLiteralString : TLiteral{
        VarString value;
        static Regex STRING = new Regex(@"^(?<qoute>'|"")(?<text>(\\\\|\\[^\\]|[^\\])*?)\k<qoute>");

        new public static TLiteralString Claim(StringClaimer claimer){
            Claim c = claimer.Claim(STRING);
            if(!c.success){
                return null;
            }
            c.Pass();
            TLiteralString str = new TLiteralString();
            str.value = new VarString(Regex.Unescape(c.GetMatch().Groups["text"].Value));
            return str;
        }

        override public Var Parse(Scope scope){
            return value;
        }
    }

}