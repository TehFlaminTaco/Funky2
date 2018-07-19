using System.Text.RegularExpressions;
using System.Text;
namespace Funky.Tokens{
    abstract class TVariable : TExpression{
        new public static TVariable Claim(StringClaimer claimer){
            Claim failTo = claimer.failPoint();
            if(TExpression.Claim(claimer) is TVariable v){
                return v;
            }
            failTo.Fail();
            return RightClaim(claimer);
        }

        // RightClaim will not try to invoke expression sensitive claims;
        public static TVariable RightClaim(StringClaimer claimer){
            return TIdentifier.Claim(claimer) as TVariable;
        }

        public static TVariable LeftClaim(StringClaimer claimer, TExpression left){
            return TIndex.LeftClaim(claimer, left) as TVariable;
        }

        override public Var Parse(Scope scope){
            return Get(scope);
        }

        public abstract Var Get(Scope scope);
        public abstract Var Set(Scope scope, Var value);
    }

    class TIndex : TVariable{
        TExpression indexed;
        TExpression index;

        private static Regex LEFT_BRACKET = new Regex(@"\[");
        private static Regex RIGHT_BRACKET = new Regex(@"\]");

        new public static TIndex LeftClaim(StringClaimer claimer, TExpression left){
            Claim c = claimer.Claim(LEFT_BRACKET);
            if(!c.success){
                return null;
            }
            TExpression index = TExpression.Claim(claimer);
            if(index == null){
                c.Fail();
                return null;
            }
            TIndex ind = new TIndex();
            ind.indexed = left;
            ind.index = index;
            claimer.Claim(RIGHT_BRACKET);

            return ind;
        }

        override public Var Get(Scope scope){
            return indexed.Parse(scope)?.Get(index.Parse(scope));
        }

        override public Var Set(Scope scope, Var value){
            return indexed.Parse(scope)?.Set(index.Parse(scope), value);
        }
    }

    class TIdentifier : TVariable{
        public string name;
        bool isLocal = false;
        static Regex LOCAL = new Regex(@"local|var|let");
        static Regex IDENTIFIER = new Regex(@"^[a-zA-Z_]\w*");

        new public static TIdentifier Claim(StringClaimer claimer){
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