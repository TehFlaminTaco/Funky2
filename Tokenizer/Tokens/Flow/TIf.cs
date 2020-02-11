using System.Text.RegularExpressions;

namespace Funky.Tokens.Flow{
    class TIf : TExpression{

        private static Regex IF = new Regex(@"if");
        private static Regex ELSE = new Regex(@"else");

        TExpression condition;
        TExpression action;
        TExpression otherwise;

        new public static TIf Claim(StringClaimer claimer){
            Claim failpoint = claimer.failPoint();
            Claim c = claimer.Claim(IF);
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
            if(action == null){
                failpoint.Fail();
                return null;
            }
            TIf ifblock = new TIf();

            ifblock.condition = condition;
            ifblock.action = action;

            c = claimer.Claim(ELSE);
            if(c.success){
                TExpression otherwise = TExpression.Claim(claimer);
                if(otherwise == null){
                    c.Fail();
                }else{
                    ifblock.otherwise = otherwise;
                    c.Pass();
                }
            }
            

            return ifblock;
        }

        override public Var Parse(Scope scope){
            Var should = condition.TryParse(scope);
            return (should.asBool()?action.TryParse(scope):otherwise?.TryParse(scope))??Var.nil;
        }
    }
}