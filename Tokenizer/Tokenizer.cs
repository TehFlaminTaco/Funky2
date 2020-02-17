using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System;
using Funky.Tokens;

namespace Funky{
    class Tokenizer{
        
    }

    public enum Escape{
        RETURN, BREAK
    }

    public struct Scope {
        public VarList variables;
        public Stack<Escaper> escape;

        public Scope(VarList v){
            variables = v;
            escape = new Stack<Escaper>();
        }
    }

    public struct Escaper{
        public Escape method;
        public Var value;

        public Escaper(Escape method, Var value){
            this.method = method;
            this.value = value;
        }
    }

    class TProgram : Token{
        List<TExpression> expressions = new List<TExpression>();
        static Regex SEMI_COLON = new Regex(@";");
        private static Regex CLEAN_ERROR = new Regex(@"\n(\n|\r|.)*");
        new public static TProgram Claim(StringClaimer claimer){
            TProgram prog = new TProgram();

            TExpression e;
            while((e = TExpression.Claim(claimer))!=null){
                claimer.Claim(SEMI_COLON);
                prog.expressions.Add(e);
            }

            if(claimer.bestReach < claimer.to_claim.Length){
                string errorString = CLEAN_ERROR.Replace(claimer.to_claim.Substring(claimer.bestReach), "...");
                throw new FunkyException($"Unexpected symbol at {claimer.getLine(claimer.bestReach)}:{claimer.getChar(claimer.bestReach)} \"{errorString}\"");
            }
            return prog;
        }

        public Var Parse(){
            VarList scopeList = new VarList();
            scopeList.parent = Globals.get();
            Scope scope = new Scope(scopeList);
            Var[] results = new Var[expressions.Count];
            Var lastRes = Var.nil;
            for(int i=0; i < expressions.Count; i++){
                lastRes = results[i] = expressions[i].TryParse(scope);
            }
            return lastRes;
        }
    }
}