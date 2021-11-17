using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System;
using Funky.Tokens;
namespace Funky.Tokens.Literal{
    [TokenIdentifier('\x0E')]
    class TLiteralList : TLiteral{

        private static Regex LEFT_BRACKET = new Regex(@"^\[");
        private static Regex RIGHT_BRACKET = new Regex(@"^\]");
        private static Regex COMMA = new Regex(@"^,");

        [InBinary] List<TArgument> arguments = new List<TArgument>();

        new public static TLiteralList Claim(StringClaimer claimer){
            Claim c = claimer.Claim(LEFT_BRACKET);
            if(!c.success){
                return null;
            }

            TLiteralList newList = new TLiteralList();

            while(true){
                Claim rb = claimer.Claim(RIGHT_BRACKET);
                if(rb.success){
                    rb.Pass();
                    break;
                }
                TArgument newArg = TArgument.Claim(claimer);
                if(newArg == null){
                    break;
                }
                newList.arguments.Add(newArg);
                claimer.Claim(COMMA);
            }

            return newList;
        }

        override public Var Parse(Scope scope){
            VarList newList = new VarList();
            int index = 0;
            for(int i=0; i < arguments.Count; i++){
                index = arguments[i].AppendArguments(newList, index, scope);
            }
            return newList;
        }
    }
}