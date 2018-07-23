using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
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


    class TLiteralStringTemplate : TLiteral{

        static Regex QOUTE = new Regex(@"`");
        static Regex ANYTHING = new Regex(@"\\\[|\\`|[^`]");
        static Regex LEFT_BRACKET = new Regex(@"\[");
        static Regex RIGHT_BRACKET = new Regex(@"\]");

        List<string> chunks = new List<string>();
        List<TExpression> dats = new List<TExpression>();

        new public static TLiteralStringTemplate Claim(StringClaimer claimer){
            Claim c = claimer.Claim(QOUTE);
            if(!c.success){
                return null;
            }
            c.Pass();
            claimer.wsignored = false;
            TLiteralStringTemplate temp = new TLiteralStringTemplate();
            StringBuilder curString = new StringBuilder();
            while(true){
                c = claimer.Claim(LEFT_BRACKET);
                if(c.success){
                    c.Pass();
                    claimer.wsignored = true;
                    TExpression exp = TExpression.Claim(claimer);
                    claimer.Claim(RIGHT_BRACKET);
                    claimer.wsignored = false;
                    temp.dats.Add(exp);
                    temp.chunks.Add(Regex.Unescape(curString.ToString()));
                    curString = new StringBuilder();
                }else if((c = claimer.Claim(ANYTHING)).success){
                    c.Pass();
                    curString.Append(c.GetText());
                }else{
                    if(curString.Length > 0){
                        temp.dats.Add(null);
                        temp.chunks.Add(Regex.Unescape(curString.ToString()));
                    }
                    claimer.Claim(QOUTE); // Optional. Because /S H R U G
                    break;
                }
            }
            claimer.wsignored = true;

            return temp;
        }

        override public Var Parse(Scope scope){
            StringBuilder sb = new StringBuilder();
            for(int i=0; i < dats.Count; i++){
                sb.Append(chunks[i]);
                if(dats[i] != null){
                    sb.Append(dats[i].Parse(scope).asString());
                }
            }
            return sb.ToString();
        }
    }

}