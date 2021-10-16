using System.Text.RegularExpressions;

namespace Funky.Tokens {
    class TCached : TLeftExpression
    {
        public TExpression body;
        public Var stored = null;
        public bool hasStored = false;
        private static Regex EXCLAIM = new Regex(@"\!");

        public override Associativity GetAssociativity(){
            return Associativity.NA;
        }

        public override TExpression GetLeft(){
            return body;
        }

        public override int GetPrecedence(){
            return -1;
        }

        new public static TCached LeftClaim(StringClaimer claimer, TExpression left){
            Claim c = claimer.Claim(EXCLAIM);
            if(!c.success)
                return null;
            TCached cached = new TCached();
            cached.SetLeft(left);
            return cached;
        }

        public override Var Parse(Scope scope){
            if(hasStored)return stored;
            hasStored = true;
            return stored = body.Parse(scope);
        }

        public override void SetLeft(TExpression newLeft){
            body = newLeft;
        }
    }
}