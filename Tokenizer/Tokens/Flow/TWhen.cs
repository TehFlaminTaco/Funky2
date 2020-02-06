using System.Text.RegularExpressions;

namespace Funky.Tokens.Flow{
    class TWhen : TExpression{

        private static Regex WITH = new Regex(@"when");

        TExpression evnt;
        TExpression block;

        new public static TWhen Claim(StringClaimer claimer){
            Claim c = claimer.Claim(WITH);

            if(!c.success)
                return null;
            
            TExpression evnt = TExpression.Claim(claimer);
            if(evnt == null){
                c.Fail();
                return null;
            }
            TExpression block = TExpression.Claim(claimer);
            if(block == null){
                c.Fail();
                return null;
            }

            TWhen when = new TWhen();
            when.evnt = evnt;
            when.block = block;
            return when;
        }

        override public Var Parse(Scope scope){
            VarEvent hookTo = evnt.Parse(scope).asEvent();
            hookTo.Hook(dat => block.Parse(scope));
            return hookTo;
        }
    }
}