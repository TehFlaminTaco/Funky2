using System.Text.RegularExpressions;

namespace Funky.Tokens.Flow{
    class TWith : TExpression{

        private static Regex WITH = new Regex(@"with");

        TExpression list;
        TExpression block;

        new public static TWith Claim(StringClaimer claimer){
            Claim c = claimer.Claim(WITH);

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

            TWith with = new TWith();
            with.list = list;
            with.block = block;
            return with;
        }

        override public Var Parse(Scope scope){
            VarList lList = list.TryParse(scope).asList();
            VarList scopeList = new VarList();
            scopeList.meta = new VarList();
            scopeList.meta["get"] = new VarFunction(dat => {
                Var resVar = lList.Get(dat._num_args[1]);
                if(resVar is VarNull)
                    resVar = scope.variables.Get(dat._num_args[1]);
                return resVar;
            });
            scopeList.meta["set"] = new VarFunction(dat => {
                Var resVar = lList.Get(dat._num_args[1]);
                if(resVar is VarNull){
                    resVar = scope.variables.Get(dat._num_args[1]);
                    if(resVar is VarNull){
                        lList.Set(dat._num_args[1], dat._num_args[2]);
                    }else{
                        scope.variables.Set(dat._num_args[1], dat._num_args[2]);
                    }
                }else{
                    lList.Set(dat._num_args[1], dat._num_args[2]);
                }
                return dat._num_args[2];
            });

            Scope subScope = new Scope(scopeList);
            subScope.escape = scope.escape;
            return block.TryParse(subScope);
        }
    }
}