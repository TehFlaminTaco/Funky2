using System.Text;
namespace Funky.Tokens{
    [TokenIdentifier('\x14')]
    class TArithmetic : TLeftExpression{

        [InBinary(optional = false)]TExpression leftArg;
        [InBinary(optional = false)]TExpression rightArg;
        [InBinary(optional = false)]TOperator op;

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

        new public static TExpression LeftClaim(StringClaimer claimer, TExpression left){
            Claim failTo = claimer.failPoint();
            TOperator operand = TOperator.Claim(claimer);
            if(operand == null){
                failTo.Fail();
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

            if(right is TLeftExpression t){
                LeftSteal ls = t.StealLeft(newArith.GetPrecedence(), newArith.GetAssociativity(), newArith);
                newArith.rightArg = ls.rightExp;
                return ls.returnExp;
            }
            
            return newArith;
        }

        override public Var Parse(Scope scope){
            return op.Parse(scope, leftArg, rightArg);
        }

    }
}