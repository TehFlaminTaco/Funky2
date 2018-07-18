using System.Text.RegularExpressions;
using System.Text;

namespace Funky.Tokens{
    class TAssignment : TExpression {
        TExpression value;
        TVariable var;
        TOperator op;

        static Regex SET = new Regex(@"=");

        new public static TAssignment Claim(StringClaimer claimer){
            Claim failTo = claimer.failPoint();

            TVariable toAssign = TVariable.Claim(claimer);
            if(toAssign == null){
                failTo.Fail();
                return null;
            }

            TOperator newOp = TOperator.Claim(claimer);

            Claim c = claimer.Claim(SET);
            if(!c.success){
                failTo.Fail();
                return null;
            }
            TExpression assignValue = TExpression.Claim(claimer);
            if(assignValue == null){
                failTo.Fail();
                return null;
            }
            TAssignment newAssign = new TAssignment();
            newAssign.var = toAssign;
            newAssign.op = newOp;
            newAssign.value = assignValue;

            return newAssign;
        }

        override public Var Parse(Scope scope){
            if(op != null){
                Var left = var.Get(scope);
                Var val = value.Parse(scope);
                val = op.Parse(left, val);
                return var.Set(scope, val);
            }else
                return var.Set(scope, value.Parse(scope));
        }
    }
}