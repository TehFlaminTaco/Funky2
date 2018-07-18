namespace Funky.Tokens{
    abstract class TExpression : Token{
        new public static TExpression claim(StringClaimer claimer){
            TExpression preClaimed = pre_claim(claimer);
            if(preClaimed == null)
                return null;
            TExpression next_claim;
            while((next_claim = post_claim(claimer, preClaimed))!=null)
                preClaimed = next_claim;
            return preClaimed;
        }

        private static TExpression pre_claim(StringClaimer claimer){
            return TAssignment.claim(claimer)   as TExpression ??
            TVariable.claim(claimer)            as TExpression ??
            TLiteral.claim(claimer)             as TExpression ??
            TParenExpression.claim(claimer)     as TExpression;
        }

        private static TExpression post_claim(StringClaimer claimer, TExpression last_claim){
            return TCall.leftClaim(claimer, last_claim);
        }

        public abstract Var Parse(Scope scope); // Although Expression requires a Parse function, it fails to implement it, because it shouldn't be possible to have a raw "TExpression" token.
    }
}