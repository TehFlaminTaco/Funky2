using System.Text.RegularExpressions;
using System.Text;

namespace Funky.Tokens{
    class TAssignment : TLeftExpression {
        TExpression value;
        TVariable var;
        TOperator op;

        static Regex SET = new Regex(@"=");

        override public void SetLeft(TExpression newLeft){
            if(newLeft is TVariable v){
                var = v;
            }
        }

        override public TExpression GetLeft(){
            return var;
        }

        override public int GetPrecedence(){
            return -1;
        }

        override public Associativity GetAssociativity(){
            return Associativity.NA;
        }

        new public static TAssignment LeftClaim(StringClaimer claimer, TExpression left){
            if(!(left is TVariable toAssign)){
                return null;
            }
            Claim failTo = claimer.failPoint();
            if(toAssign == null){
                failTo.Fail();
                return null;
            }

            TOperator newOp = TOperator.Claim(claimer);

            if(newOp != null && (newOp.op.name == "lt" || newOp.op.name == "gt")){
                failTo.Fail();
                return null;
            }

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
                Var val = op.Parse(scope, var, value);
                return var.Set(scope, val);
            }else
                return var.Set(scope, value.Parse(scope));
        }
    }
}