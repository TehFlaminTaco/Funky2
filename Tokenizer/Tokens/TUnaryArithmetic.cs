namespace Funky.Tokens{
    class TUnaryArithmetic : TExpression{
        TUnaryOperator op = null;
        TExpression arg = null;
        new public static TExpression Claim(StringClaimer claimer){
            Claim failPoint = claimer.failPoint();
            TUnaryOperator op = TUnaryOperator.Claim(claimer);
            if(op != null){
                TExpression exp = TExpression.Claim(claimer);
                if(exp == null){
                    failPoint.Fail();
                    return null;
                }
                TUnaryArithmetic arith = new TUnaryArithmetic();
                arith.op = op;
                if(exp is TLeftExpression l){
                    LeftSteal ls = l.StealLeft(op.op.precedence, Associativity.RIGHT_TO_LEFT, arith);
                    arith.arg = ls.rightExp;
                    return ls.returnExp;
                }else{
                    arith.arg = exp;
                    return arith;
                }
            }
            return null;
        }

        override public Var Parse(Scope scope){
            return op.Parse(arg.Parse(scope));
        }
    }
}