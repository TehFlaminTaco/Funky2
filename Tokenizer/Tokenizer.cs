using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System;
using Funky.Tokens;

namespace Funky{
    class Tokenizer{
        
    }

    enum Escape{
        RETURN, BREAK
    }

    struct Scope {
        public VarList variables;

        public Scope(VarList v){
            variables = v;
        }
    }

    struct Escaper{
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
        new public static TProgram Claim(StringClaimer claimer){
            TProgram prog = new TProgram();

            TExpression e;
            while((e = TExpression.Claim(claimer))!=null){
                claimer.Claim(SEMI_COLON);
                prog.expressions.Add(e);
            }
            return prog;
        }

        public void Parse(){
            VarList scopeList = new VarList();
            scopeList.parent = Globals.get();
            Scope scope = new Scope(scopeList);
            Var[] results = new Var[expressions.Count];
            for(int i=0; i < expressions.Count; i++){
                results[i] = expressions[i].Parse(scope);
            }
            return;
        }
    }
}