using System.Text.RegularExpressions;

namespace Funky.Tokens{
    class TAssignment : TExpression {
        TExpression value;
        TVariable var;
        //TOperator operator;

        static Regex SET = new Regex(@"=");

        new public static TAssignment claim(StringClaimer claimer){
            Claim failTo = claimer.failPoint();

            TVariable toAssign = TVariable.claim(claimer);
            if(toAssign == null){
                return null;
            }
            Claim c = claimer.Claim(SET);
            if(!c.success){
                failTo.Fail();
                return null;
            }
            TExpression assignValue = TExpression.claim(claimer);
            if(assignValue == null){
                failTo.Fail();
                return null;
            }
            TAssignment newAssign = new TAssignment();
            newAssign.var = toAssign;
            newAssign.value = assignValue;

            return newAssign;
        }

        override public Var Parse(Scope scope){
            return var.Set(scope, value.Parse(scope));
        }
    }
}