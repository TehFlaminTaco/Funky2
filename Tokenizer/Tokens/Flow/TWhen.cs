using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Funky.Tokens.Flow{
    [TokenIdentifier('\x0B')]
    class TWhen : TExpression{

        private static Regex WITH = new Regex(@"^when");

        [InBinary(optional = false)]TExpression evnt;
        [InBinary(optional = false)]TExpression block;

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
            VarEvent hookTo = evnt.TryParse(scope).asEvent();
            
            hookTo.Hook(dat => {
                Scope newScope = new Scope();
                newScope.variables = new VarList();
                newScope.variables.parent = scope.variables;
                newScope.variables.meta = new VarList();
                newScope.escape = new Stack<Escaper>();
                foreach(var kv in dat._num_args)
                    newScope.variables.double_vars[kv.Key] = kv.Value;
                foreach(var kv in dat._str_args)
                    newScope.variables.string_vars[kv.Key] = kv.Value;
                foreach(var kv in dat._var_args)
                    newScope.variables.other_vars[kv.Key] = kv.Value;
                return block.TryParse(newScope);
            });
            return hookTo;
        }
    }
}