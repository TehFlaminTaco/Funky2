using System.Text.RegularExpressions;
namespace Funky.Tokens{
    class TDeoperator : TExpression {
        private static Regex ATSYMBOL = new Regex(@"@");

        TOperator operand;
        TExpression expr;

        override public Var Parse(Scope scope){
            if(operand != null){
                VarFunction func = new VarFunction(dat => {
                    return operand.Parse(dat.num_args[0], dat.num_args[1]);
                });
                func.FunctionText = raw;
                return func;
            }
            if(expr != null){
                VarFunction func = null;
                func = new VarFunction(dat => {
                    return expr.Parse(func.scope);
                });
                func.scope = scope;
                func.FunctionText = raw;
                return func;
            }
            return Var.nil;
        }

        new public static TDeoperator Claim(StringClaimer claimer){
            int start = claimer.Location();
            Claim c = claimer.Claim(ATSYMBOL);
            if(!c.success){
                return null;
            }

            TDeoperator deop = new TDeoperator();
            TOperator operand = TOperator.Claim(claimer);
            if(operand != null){
                deop.operand = operand;
                c.Pass();
                deop.raw = claimer.SubString(start);
                return deop;
            }else{
                TExpression expr = TExpression.Claim(claimer);
                if(expr != null){
                    deop.expr = expr;
                    c.Pass();
                    deop.raw = claimer.SubString(start);
                    return deop;
                }
            }
            c.Fail();
            return null;
        }
    }
}