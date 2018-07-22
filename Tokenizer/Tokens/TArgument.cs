using System.Text.RegularExpressions;
using System.Collections.Generic;
using Funky.Tokens.Literal;
namespace Funky.Tokens{
    abstract class TArgument : TExpression{
        public abstract int AppendArguments(VarList argumentList, int index, Scope scope);
        override public Var Parse(Scope scope){return null;} // Never parse. Never.

        new public static TArgument Claim(StringClaimer claimer){
            return TArgSplat.Claim(claimer)    as TArgument ??
            TArgAssign.Claim(claimer)          as TArgument ??
            TArgExpression.Claim(claimer)      as TArgument;
        } 
    }

    class TArgExpression : TArgument{
        TExpression heldExp;
        override public int AppendArguments(VarList argumentList, int index, Scope scope){
            argumentList.double_vars[index] = heldExp.Parse(scope);
            return index+1;
        }

        new public static TArgExpression Claim(StringClaimer claimer){
            TExpression heldExpr = TExpression.Claim(claimer);
            if(heldExpr == null)
                return null;
            TArgExpression newArgExp = new TArgExpression();
            newArgExp.heldExp = heldExpr;
            return newArgExp;
        }
    }

    class TArgSplat : TArgument {
        TExpression heldExp;
        private static Regex SPLAT = new Regex(@"\.\.\.");


        override public int AppendArguments(VarList argumentList, int index, Scope scope){
            Var v = heldExp.Parse(scope);
            if(v is VarList list){
                int max = -1;
                for(int i=0; list.double_vars.ContainsKey(i); i++){
                    argumentList.double_vars[index++] = list.double_vars[i];
                    max = i;
                }
                foreach(KeyValuePair<double, Var> kv in list.double_vars){
                    if(kv.Key%1 == 0.0d && kv.Key >= 0 && kv.Key < max)
                        continue;
                    argumentList.double_vars[kv.Key] = kv.Value;
                }
                foreach(KeyValuePair<string, Var> kv in list.string_vars)
                    argumentList.string_vars[kv.Key] = kv.Value;
                foreach(KeyValuePair<Var, Var> kv in list.other_vars)
                    argumentList.other_vars[kv.Key] = kv.Value;
                
            }
            return index;
        }

        new public static TArgSplat Claim(StringClaimer claimer){
            Claim failPoint = claimer.failPoint();
            
            TExpression listExp = TExpression.Claim(claimer);
            if(listExp == null){
                failPoint.Fail();
                return null;
            }

            Claim splatDots = claimer.Claim(SPLAT);
            if(!splatDots.success){
                failPoint.Fail();
                return null;
            }

            TArgSplat splat = new TArgSplat();
            splat.heldExp = listExp;
            return splat;
        }
    }

    class TArgAssign : TArgument{
        TLiteral literalVar;
        TIdentifier textVar;
        TExpression value;

        private static Regex EQUALS = new Regex(@"=");


        override public int AppendArguments(VarList argumentList, int index, Scope scope){
            if(literalVar!=null){
                argumentList.Set(literalVar.Parse(scope), value.Parse(scope));
            }else{
                argumentList.Set(textVar.name, value.Parse(scope));
            }
            return index;
        }


        new public static TArgAssign Claim(StringClaimer claimer){
            Claim failPoint = claimer.failPoint();
            TArgAssign assn = new TArgAssign();
            assn = ClaimLiteral(claimer, assn) ?? ClaimIdentifier(claimer, assn);
            if(assn == null){
                failPoint.Fail();
                return null;
            }
            Claim equals = claimer.Claim(EQUALS);
            if(!equals.success){
                failPoint.Fail();
                return null;
            }
            TExpression val = TExpression.Claim(claimer);
            if(val == null){
                failPoint.Fail();
                return null;
            }
            assn.value = val;
            return assn;
        }

        private static TArgAssign ClaimLiteral(StringClaimer claimer, TArgAssign assn){
            TLiteral lit = TLiteral.Claim(claimer);
            if(lit != null){
                assn.literalVar = lit;
                return assn;
            }
            return null;
        }

        private static TArgAssign ClaimIdentifier(StringClaimer claimer, TArgAssign assn){
            TIdentifier ident = TIdentifier.Claim(claimer);
            if(ident != null){
                assn.textVar = ident;
                return assn;
            }
            return null;
        }
    }
}