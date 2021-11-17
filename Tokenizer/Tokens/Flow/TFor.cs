using Funky.Tokens;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Funky.Tokens.Flow{
    [TokenIdentifier('\x03')]
    class TFor : TExpression{
        private static Regex FOR = new Regex(@"^for");
        private static Regex LEFT_BRACKET = new Regex(@"^\(");
        private static Regex RIGHT_BRACKET = new Regex(@"^\)");
        private static Regex SEMI_COLON = new Regex(@"^;");

        [InBinary]
        TExpression initial;
        [InBinary]
        TExpression condition;
        [InBinary]
        TExpression after;
        [InBinary]
        TExpression body;
        
        new public static TFor Claim(StringClaimer claimer){
            Claim c = claimer.Claim(FOR);
            if(!c.success)
                return null;

            TExpression[] exprs = new TExpression[4];
            claimer.Claim(LEFT_BRACKET);
            int claimed = 0;
            for(;claimed < 3; claimed++){
                if((exprs[claimed] = TExpression.Claim(claimer))==null){ // Out of Expressions. :(
                    break;
                }
                claimer.Claim(SEMI_COLON);
            }
            claimed--;
            c = claimer.Claim(RIGHT_BRACKET);
            if((exprs[3] = TExpression.Claim(claimer))==null && !c.success){
                if(claimed >= 0 && exprs[claimed] != null){
                    exprs[3] = exprs[claimed];
                    exprs[claimed] = null;
                }
            }
            TFor newFor = new TFor();
            newFor.initial = exprs[0];
            newFor.condition = exprs[1];
            newFor.after = exprs[2];
            newFor.body = exprs[3];

            return newFor;
        }

        override public Var Parse(Scope scope){
            initial?.TryParse(scope);
            Var ret = Var.nil;
            while(condition == null || (condition.TryParse(scope).asBool())){
                ret = body?.TryParse(scope)??ret;
                if(scope.escape.Count > 0){
                    Escaper esc = scope.escape.Peek();
                    if(esc.method == Escape.BREAK)
                        scope.escape.Pop();
                    return esc.value;
                }
                after?.TryParse(scope);
            }

            return ret;
        }
    }
}