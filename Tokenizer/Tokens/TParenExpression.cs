using System.Text.RegularExpressions;
using System.Text;
namespace Funky.Tokens{
    class TParenExpression : TExpression{
        private static Regex LEFT_BRACKET = new Regex("^\\(");
        private static Regex RIGHT_BRACKET = new Regex("^\\)");

        TExpression realExpression;

        new public static TParenExpression Claim(StringClaimer claimer){
            Claim lb = claimer.Claim(LEFT_BRACKET);
            if(!lb.success) return null;
            TParenExpression ptoken = new TParenExpression();
            ptoken.realExpression = TExpression.Claim(claimer);
            if(ptoken.realExpression == null){
                lb.Fail();
                return null;
            }
            lb.Pass();
            Claim rb = claimer.Claim(RIGHT_BRACKET);
            if(rb.success) rb.Pass(); // right bracket is optional. So just pass it if we get it.
            return ptoken;
        }

        override public Var Parse(Scope scope){
            return realExpression.TryParse(scope);
        }
    }
}