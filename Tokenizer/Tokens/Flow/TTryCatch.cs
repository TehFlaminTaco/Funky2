using Funky;
using System.Text.RegularExpressions;

namespace Funky.Tokens{
    [TokenIdentifier('\x09')]
    class TTryCatch : TExpression{
        [InBinary(optional = false)] public TExpression body = null;
        [InBinary] public TIdentifier catchTarget = null;
        [InBinary] public TExpression catchBody = null;
        
        private static Regex TRY = new Regex(@"^try");
        private static Regex LEFT_BRACKET = new Regex(@"^\(");
        private static Regex RIGHT_BRACKET = new Regex(@"^\)");
        private static Regex CATCH = new Regex(@"^catch");
        new public static TTryCatch Claim(StringClaimer claimer){
            Claim c = claimer.Claim(TRY);
            if(!c.success)
                return null;
            
            TExpression body = TExpression.Claim(claimer);
            if(body == null){
                c.Fail();
                return null;
            }

            TTryCatch tryCatch = new TTryCatch();
            tryCatch.body = body;
            Claim noCatch = claimer.failPoint();
            claimer.Claim(LEFT_BRACKET);

            if(claimer.Claim(CATCH).success){
                TIdentifier catchTarget = TIdentifier.Claim(claimer);
                claimer.Claim(RIGHT_BRACKET);
                TExpression catchBody = TExpression.Claim(claimer);
                if(catchBody == null){
                    noCatch.Fail();
                }else{
                    tryCatch.catchTarget = catchTarget;
                    tryCatch.catchBody = catchBody;
                }
            }else{
                noCatch.Fail();
            }

            return tryCatch;
        }

        public override Var Parse(Scope scope){
            try{
                return body.TryParse(scope);
            }catch(System.Exception e){
                if(catchBody != null){
                    Scope subscope = new Scope();
                    subscope.escape = scope.escape;
                    subscope.variables = new VarList();
                    subscope.variables.meta = new VarList();
                    subscope.variables.parent = scope.variables;
                    if(catchTarget != null){
                        catchTarget.isLocal = true;
                        catchTarget.Set(scope, e.Message);
                    }
                    return catchBody.TryParse(subscope);
                }
                return Var.nil;
            }
        }
    }
}