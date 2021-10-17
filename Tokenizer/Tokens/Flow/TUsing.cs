using System.Text.RegularExpressions;

namespace Funky.Tokens.Flow{
    class TUsing : TExpression{

        private static Regex USING = new Regex(@"using");

        TExpression list;
        TExpression block;

        new public static TUsing Claim(StringClaimer claimer){
            Claim c = claimer.Claim(USING);

            if(!c.success)
                return null;
            
            TExpression list = TExpression.Claim(claimer);
            if(list == null){
                c.Fail();
                return null;
            }
            TExpression block = TExpression.Claim(claimer);
            if(block == null){
                c.Fail();
                return null;
            }

            TUsing usng = new TUsing();
            usng.list = list;
            usng.block = block;
            return usng;
        }

        override public Var Parse(Scope scope){
            var touse = list.TryParse(scope);
            try {
                var outp = block.TryParse(scope);
                touse.Dispose();
                return outp;
            }catch(System.Exception e){
                touse.Dispose();
                throw e;
            }
        }
    }
}