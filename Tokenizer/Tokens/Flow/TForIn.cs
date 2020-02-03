using Funky.Tokens;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Funky.Tokens.Flow{
    class TForIn : TExpression{
        private static Regex FOR = new Regex(@"for");
        private static Regex LEFT_BRACKET = new Regex(@"^\(");
        private static Regex RIGHT_BRACKET = new Regex(@"^\)");
        private static Regex IN = new Regex(@"in");

        TVariable inVar;
        TExpression iter;
        TExpression body;

        new public static TForIn Claim(StringClaimer claimer){
            Claim c = claimer.Claim(FOR);
            if(!c.success)
                return null;

            claimer.Claim(LEFT_BRACKET);
            TVariable v = TVariable.Claim(claimer);
            if(v == null){
                c.Fail();
                return null;
            }

            if(!claimer.Claim(IN).success){
                c.Fail();
                return null;
            }

            TExpression iter = TExpression.Claim(claimer);
            if(iter == null){
                c.Fail();
                return null;
            }
            claimer.Claim(RIGHT_BRACKET);

            TExpression body = TExpression.Claim(claimer);
            if(body == null){
                c.Fail();
                return null;
            }

            TForIn newFor = new TForIn();
            newFor.inVar = v;
            newFor.body = body;
            newFor.iter = iter;
            
            return newFor;
        }

        override public Var Parse(Scope scope){
            Var iterVar = iter.Parse(scope);

            Var ret = Var.nil;

            if(iterVar is VarList){
                VarList vl = iterVar.asList();
                foreach(KeyValuePair<double, Var> kv in vl.double_vars){
                    inVar.Set(scope, kv.Key);
                    ret = body.Parse(scope);
                }
                foreach(KeyValuePair<string, Var> kv in vl.string_vars){
                    inVar.Set(scope, kv.Key);
                    ret = body.Parse(scope);
                }
                foreach(KeyValuePair<Var, Var> kv in vl.other_vars){
                    inVar.Set(scope, kv.Key);
                    ret = body.Parse(scope);
                }
            }else if(iterVar is VarString){
                string vs = iterVar.asString().data;
                for(int i=0; i < vs.Length; i++){
                    inVar.Set(scope, ""+vs[i]);
                    ret = body.Parse(scope);
                }
            }else if(iterVar is VarNumber){
                int vi = (int)iterVar.asNumber().value;
                for(int i=0; i < vi; i++){
                    inVar.Set(scope, vi);
                    ret = body.Parse(scope);
                }
            }else if(iterVar is VarFunction){
                VarFunction func = (VarFunction)iterVar;
                Var fr;
                CallData cd = new CallData();
                cd.num_args = new Dictionary<double, Var>();
                cd.str_args = new Dictionary<string, Var>();
                cd.var_args = new Dictionary<Var,    Var>();
                while (!((fr = func.Call(cd)) is VarNull)){
                    inVar.Set(scope, fr);
                    ret = body.Parse(scope);
                }
            }

            return ret;

        }
    }
}