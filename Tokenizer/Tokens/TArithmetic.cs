using System.Text;
namespace Funky.Tokens{
    class TArithmetic : TLeftExpression{

        TExpression leftArg;
        TExpression rightArg;
        TOperator op;

         override public TExpression GetLeft(){
            return leftArg;
        }

        override public void SetLeft(TExpression newLeft){
            leftArg = newLeft;
        }

        override public int GetPrecedence(){
            return op.GetPrecedence();
        }

        override public Associativity GetAssociativity(){
            return op.GetAssociativity();
        }

        new public static TLeftExpression leftClaim(StringClaimer claimer, TExpression left){
            Claim failTo = claimer.failPoint();
            TOperator operand = TOperator.Claim(claimer);
            if(operand == null){
                return null;
            }
            TExpression right = TExpression.Claim(claimer);
            if(right == null){
                failTo.Fail();
                return null;
            }

            TArithmetic newArith = new TArithmetic();
            newArith.leftArg = left;
            newArith.rightArg = right;
            newArith.op = operand;

            if(right is TLeftExpression t && t.GetAssociativity() != Associativity.NA){
                int prec = operand.GetPrecedence();
                int r_prec = t.GetPrecedence();
                if (prec < r_prec || (prec == r_prec && operand.GetAssociativity() == Associativity.LEFT_TO_RIGHT)){
                    newArith.rightArg = t.GetLeft();
                    t.SetLeft(newArith);
                    return t;
                }
            }
            
            return newArith;
        }

        override public Var Parse(Scope scope){
            return op.Parse(leftArg.Parse(scope), rightArg.Parse(scope));
        }

    }
}