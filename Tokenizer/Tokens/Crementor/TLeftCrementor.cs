namespace Funky.Tokens{
    class TLeftCrementor : TLeftExpression{
        TVariable var;
        TOperator op;

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

        new public static TLeftCrementor LeftClaim(StringClaimer claimer, TExpression left){
            if(left is TVariable v)
                return  LeftClaim(claimer, v, new SOperator(@"\+\+", "add", -1)) ??
                        LeftClaim(claimer, v, new SOperator(@"\-\-", "sub", -1));
            return null;
        }

        public static TLeftCrementor LeftClaim(StringClaimer claimer, TVariable left, SOperator op){
            Claim c = claimer.Claim(op.regex);
            if(!c.success){
                return null;
            }
            c.Pass();
            TLeftCrementor crementor = new TLeftCrementor();
            TOperator tOp = new TOperator();
            tOp.op = op;
            crementor.op = tOp;
            crementor.var = left;
            return crementor;
        }

        override public Var Parse(Scope scope){
            Var ret = var.Get(scope);
            var.Set(scope, op.Parse(var.Get(scope), new VarNumber(1)));
            return ret;
        }
    }
}