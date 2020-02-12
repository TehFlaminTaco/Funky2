using Funky.Tokens.Flow;
using Funky.Tokens.Literal;
namespace Funky.Tokens{
    abstract class TExpression : Token{
        new public static TExpression Claim(StringClaimer claimer){
            int claimStart = claimer.currentPoint();
            TExpression preClaimed = pre_claim(claimer);
            if(preClaimed == null)
                return null;
            preClaimed.SetDebugInfo(claimer, claimStart);
            TExpression next_claim;
            while((next_claim = post_claim(claimer, preClaimed))!=null){
                preClaimed = next_claim;
                preClaimed.SetDebugInfo(claimer, claimStart);
            }
            return preClaimed;
        }

        private static TExpression pre_claim(StringClaimer claimer){
            return TIf.Claim(claimer)           as TExpression ??
            TFunction.Claim(claimer)            as TExpression ??
            TForIn.Claim(claimer)               as TExpression ??
            TFor.Claim(claimer)                 as TExpression ??
            TWhile.Claim(claimer)               as TExpression ??
            TWith.Claim(claimer)                as TExpression ??
            TWhen.Claim(claimer)                as TExpression ??
            TTryCatch.Claim(claimer)            as TExpression ??
            TNil.Claim(claimer)                 as TExpression ??
            TReturn.Claim(claimer)              as TExpression ??
            TVariable.RightClaim(claimer)       as TExpression ??
            TLiteral.Claim(claimer)             as TExpression ??
            TRightCrementor.Claim(claimer)      as TExpression ??
            TUnaryArithmetic.Claim(claimer)     as TExpression ??
            TParenExpression.Claim(claimer)     as TExpression ??
            TBlock.Claim(claimer)               as TExpression ??
            TDeoperator.Claim(claimer)          as TExpression;
        }

        private static TExpression post_claim(StringClaimer claimer, TExpression last_claim){
            return  TVariable.LeftClaim(claimer, last_claim)as TExpression ??
            TCall.LeftClaim(claimer, last_claim)            as TExpression ??
            TAssignment.LeftClaim(claimer, last_claim)      as TExpression ??
            TVariable.LeftClaim(claimer, last_claim)        as TExpression ??
            TLeftCrementor.LeftClaim(claimer, last_claim)   as TExpression ??
            TArithmetic.LeftClaim(claimer, last_claim)      as TExpression;
        }

        public abstract Var Parse(Scope scope); // Although Expression requires a Parse function, it fails to implement it, because it shouldn't be possible to have a raw "TExpression" token.

        public Var TryParse(Scope scope){
            try{
                return Parse(scope);
            }catch(System.Exception e){
                ShowError(e);
            }
            return Var.nil;
        }
    }
}