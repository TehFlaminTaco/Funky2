using System.Text.RegularExpressions;
namespace Funky.Tokens{
    class TReturn : TExpression{
        private static Regex RETURN = new Regex(@"return");
        private static Regex BREAK = new Regex(@"break");

        TExpression exp;
        Escape escType;

        new public static TReturn Claim(StringClaimer claimer){
            Claim c;
            if((c = claimer.Claim(RETURN)).success){
                c.Pass();
                TReturn ret = new TReturn();
                ret.exp = TExpression.Claim(claimer);
                ret.escType = Escape.RETURN;
                return ret;
            }
            if((c = claimer.Claim(BREAK)).success){
                c.Pass();
                TReturn ret = new TReturn();
                ret.exp = TExpression.Claim(claimer);
                ret.escType = Escape.BREAK;
                return ret;
            }
            return null;
        }
        override public Var Parse(Scope scope){
            Var o = exp?.TryParse(scope) ?? Var.nil;
            scope.escape.Push(new Escaper(escType, o));
            return o;
        }
    }
}