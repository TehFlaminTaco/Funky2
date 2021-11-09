using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using Funky.Tokens.Literal;

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

        private static Regex LEFT_BRACKET = new Regex(@"^\[");
        private static Regex RIGHT_BRACKET = new Regex(@"^\]");
        private static Regex DOT = new Regex(@"^[.:]");

        public bool curry = false;

        new public static TIndex LeftClaim(StringClaimer claimer, TExpression left){
            Claim c = claimer.Claim(LEFT_BRACKET);
            if(c.success){
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
            }else{
                c = claimer.Claim(DOT);
                if(c.success){
                    TIdentifier ident = TIdentifier.Claim(claimer);
                    if(ident == null){
                        c.Fail();
                        return null;
                    }
                    TIndex ind = new TIndex();
                    TLiteralString indexName = new TLiteralString();
                    indexName.value = new VarString(ident.name);
                    ind.indexed = left;
                    ind.index = indexName;
                    ind.curry = c.GetText()==":";
                    return ind;
                }else{
                    return null;
                }
            }
        }

        override public Var Get(Scope scope){
            if(curry){
                return new VarFunction(dat => {
                    Var frm = indexed.TryParse(scope);
                    
                    Var oldFunc = frm?.Get(index.TryParse(scope));

                    CallData newCD = new CallData();
                    newCD._num_args = new Dictionary<double, Var>();
                    newCD._str_args = new Dictionary<string, Var>();
                    newCD._var_args = new Dictionary<Var,    Var>();

                    foreach(KeyValuePair<string, Var> kv in dat._str_args)
                        newCD._str_args[kv.Key] = kv.Value;
                    foreach(KeyValuePair<Var, Var> kv in dat._var_args)
                        newCD._var_args[kv.Key] = kv.Value;
                    foreach(KeyValuePair<double, Var> kv in dat._num_args)
                        newCD._num_args[kv.Key+1] = kv.Value;
                    newCD._num_args[0d] = frm;

                    return oldFunc.Call(newCD);
                });
            }else
                return indexed.TryParse(scope)?.Get(index.TryParse(scope));
        }

        override public Var Set(Scope scope, Var value){
            return indexed.TryParse(scope)?.Set(index.TryParse(scope), value);
        }
    }

    class TIdentifier : TVariable{
        public string name;
        public bool isLocal = false;
        static Regex LOCAL = new Regex(@"^local|^var|^let");
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
            if(isLocal && !scope.variables.string_vars.ContainsKey(name)){
                scope.variables.string_vars[name] = Var.undefined;
            }
            return isLocal ? scope.variables.string_vars[name] : scope.variables.Get(name);
        }

        public override Var Set(Scope scope, Var value){
            return isLocal ? scope.variables.string_vars[name] = value : scope.variables.Set(name, value);
        }
    }
}