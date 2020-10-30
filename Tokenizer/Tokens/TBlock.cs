using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Funky.Tokens{
    class TBlock : TExpression {
        List<TExpression> expressions = new List<TExpression>();

        private static Regex LEFT_BRACKET = new Regex(@"\{");
        private static Regex RIGHT_BRACKET = new Regex(@"\}");
        private static Regex SEMI_COLON = new Regex(@";");

        override public Var Parse(Scope scope){
            Var ret = Var.nil;
            Scope newScope = new Scope();
            newScope.variables = new VarList();
            newScope.variables.parent = scope.variables;
            newScope.variables.meta = new VarList();
            newScope.escape = scope.escape;
            for(int i=0; i < expressions.Count; i++){
                if(scope.escape.Count > 0){
                    return scope.escape.Peek().value;
                }
                ret = expressions[i].TryParse(newScope);
            }
            return ret;
        }

        new public static TBlock Claim(StringClaimer claimer){
            Claim c = claimer.Claim(LEFT_BRACKET);
            if(!c.success){
                return null;
            }
            c.Pass();
            TBlock newBlock = new TBlock();
            TExpression nExp;
            if((c = claimer.Claim(RIGHT_BRACKET)).success){
                c.Pass();
                return newBlock;
            }
            while((nExp = TExpression.Claim(claimer)) != null){
                newBlock.expressions.Add(nExp);
                claimer.Claim(SEMI_COLON);
                if((c = claimer.Claim(RIGHT_BRACKET)).success){
                    c.Pass();
                    return newBlock;
                }
            }
            return newBlock;
        }
    }
}