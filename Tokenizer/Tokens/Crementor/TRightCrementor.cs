namespace Funky.Tokens{
    class TRightCrementor : TExpression{
        TVariable var;
        TOperator op;

        new public static TRightCrementor Claim(StringClaimer claimer){
            return  RightClaim(claimer, new SOperator(@"\+\+", "add", -1)) ??
                    RightClaim(claimer, new SOperator(@"\-\-", "sub", -1));
        }

        public static TRightCrementor RightClaim(StringClaimer claimer, SOperator op){
            Claim c = claimer.Claim(op.regex);
            if(!c.success){
                return null;
            }
            TVariable right = TVariable.Claim(claimer);
            if(right == null){
                c.Fail();
                return null;
            }
            c.Pass();
            TRightCrementor crementor = new TRightCrementor();
            TOperator tOp = new TOperator();
            tOp.op = op;
            crementor.op = tOp;
            crementor.var = right;
            return crementor;
        }

        override public Var Parse(Scope scope){
            var.Set(scope, op.Parse(var.Get(scope), new VarNumber(1)));
            return var.Get(scope);
        }

        static char _ = RegisterTokenType('\x02', typeof(TRightCrementor));

        public override void TokenToBinary(System.IO.BinaryWriter writer)
        {
            WriteToken(writer, var);
            WriteToken(writer, op);
        }

        public override Token BinaryToToken(System.IO.BinaryReader reader)
        {
            var = ReadToken(reader) as TVariable ?? throw new System.ArgumentException("Expected TVariable");
            op = ReadToken(reader) as TOperator ?? throw new System.ArgumentException("Expected TOperator");
            return this;
        }
    }
}