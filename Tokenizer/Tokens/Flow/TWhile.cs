using Funky.Tokens;
using System.Text.RegularExpressions;
namespace Funky.Tokens.Flow{
    class TWhile : TExpression{

        private static Regex WHILE = new Regex(@"^while");

        TExpression condition;
        TExpression action;

        new public static TWhile Claim(StringClaimer claimer){
            Claim failpoint = claimer.failPoint();
            Claim c = claimer.Claim(WHILE);
            if(!c.success){
                failpoint.Fail();
                return null;
            }
            TExpression condition = TExpression.Claim(claimer);
            if(condition == null){
                failpoint.Fail();
                return null;
            }
            TExpression action = TExpression.Claim(claimer);
            if(condition == null){
                failpoint.Fail();
                return null;
            }
            TWhile whileBlock = new TWhile();

            whileBlock.condition = condition;
            whileBlock.action = action;
            return whileBlock;
        }

        override public Var Parse(Scope scope){
            Var ret = Var.nil;
            while(condition.TryParse(scope).asBool()){
                ret = action.TryParse(scope);
                if(scope.escape.Count > 0){
                    Escaper esc = scope.escape.Peek();
                    if(esc.method == Escape.BREAK)
                        scope.escape.Pop();
                    return esc.value;
                }
            }
            return ret;
        }
    }
}