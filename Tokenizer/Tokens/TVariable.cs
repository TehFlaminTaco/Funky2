using System.Text.RegularExpressions;
namespace Funky.Tokens{
    abstract class TVariable : TExpression{
        new public static TVariable claim(StringClaimer claimer){
            TVariable result;
            if((result = TIdentifier.claim(claimer))!=null)return result;
            return null;
        }

        override public Var Parse(Scope scope){
            return Get(scope);
        }

        public abstract Var Get(Scope scope);
        public abstract Var Set(Scope scope, Var value);
    }

    class TIdentifier : TVariable{
        public string name;
        bool isLocal = false;
        static Regex LOCAL = new Regex(@"local|var|let");
        static Regex IDENTIFIER = new Regex(@"^[a-zA-Z_]\w*");

        new public static TIdentifier claim(StringClaimer claimer){   
            TIdentifier ident = new TIdentifier();
            Claim c = claimer.Claim(LOCAL);
            if(c.success){
                c.Pass();
                ident.isLocal = true;
            }

            c = claimer.Claim(IDENTIFIER);
            if(!c.success){
                return null;
            }
            ident.name = c.GetText();
            return ident;
        }

        public override Var Get(Scope scope){
            return isLocal ? (scope.variables.string_vars.ContainsKey(name) ? scope.variables.string_vars[name] : null) : scope.variables.Get(name);
        }

        public override Var Set(Scope scope, Var value){
            return isLocal ? scope.variables.string_vars[name] = value : scope.variables.Set(name, value);
        }
        
    }
}