using System.Text.RegularExpressions;
namespace Funky.Tokens{
    class TNil : TExpression {
        VarNull nilType;

        public TNil(VarNull nilType){
            this.nilType = nilType;
        }

        private static Regex NIL = new Regex(@"nil");
        private static Regex UNDEFINED = new Regex(@"undefined");

        new public static TNil Claim(StringClaimer claimer){
            Claim c = claimer.Claim(NIL);
            if(c.success){
                c.Pass();
                return new TNil(Var.nil);
            }
            c = claimer.Claim(UNDEFINED);
            if(c.success){
                c.Pass();
                return new TNil(Var.undefined);
            }
            return null;
        }

        override public Var Parse(Scope scope){
            return nilType;
        }
    }
}