// I cannot be prepared enough to write this god-forsaken token.

//  MOST info a function can have is:
//  NAME
//      TVariable
//      Only applicable if func was to the left, and argument names are in Parenthesis.
//  BODY
//      TExpression
//      Can't not be used. The thing that does the run.
//  ARGUMENTS
//      List<TArgNamer>
//      Can be empty.
//      HOW HOW HOW HOW HOW?

// Function Definition Methods
// a => b
// (a, b) => c
// function(){}
// function f(){}


using System.Text.RegularExpressions;
using System.Collections.Generic;
using Funky.Tokens;

namespace Funky.Tokens.Flow{
    class TFunction : TExpression{
        private static Regex FUNCTION = new Regex(@"func(tion)?");
        private static Regex POINTER = new Regex(@"=>");
        private static Regex LEFT_BRACKET = new Regex(@"\(");
        private static Regex RIGHT_BRACKET = new Regex(@"\)");
        private static Regex COMMA = new Regex(@",");

        TExpression body;
        TVariable name;
        List<TArgNamer> args;

        new public static TFunction Claim(StringClaimer claimer){
            Claim failPoint = claimer.failPoint();
            int startPoint = claimer.Location();
            Claim c;
            if((c = claimer.Claim(FUNCTION)).success){ // Chunky form.
                TVariable name = TVariable.Claim(claimer); // Surprisingly optional.
                List<TArgNamer> args = ArgList(claimer);
                if(args == null){
                    failPoint.Fail();
                    return null;
                }
                TExpression body = TExpression.Claim(claimer);
                if(body == null){
                    failPoint.Fail();
                    return null;
                }
                TFunction func = new TFunction();
                func.name = name;
                func.body = body;
                func.args = args;
                func.raw = claimer.SubString(startPoint);
                return func; // OH MY, A SUCCESSFUL RETURN. THE GODS BE PRAISED.
            }else{ // Lightweight form.
                TArgVariable ident = TArgVariable.Claim(claimer);
                List<TArgNamer> args;
                if(ident == null){
                    args = ArgList(claimer);
                    if(args == null){
                        failPoint.Fail();
                        return null;
                    }
                }else{
                    args = new List<TArgNamer>();
                    args.Add(ident);
                }
                c = claimer.Claim(POINTER);
                if(!c.success){
                    failPoint.Fail();
                    return null;
                }
                TExpression body = TExpression.Claim(claimer);
                if(body == null){
                    failPoint.Fail();
                    return null;
                }
                TFunction func = new TFunction();
                func.body = body;
                func.args = args;
                func.raw = claimer.SubString(startPoint);
                return func;
            }
        }

        private static List<TArgNamer> ArgList(StringClaimer claimer){
            Claim lb = claimer.Claim(LEFT_BRACKET);
            if(!lb.success){
                return null;
            }
            lb.Pass();
            List<TArgNamer> args = new List<TArgNamer>();
            TArgNamer n;
            while((n=SingleArg(claimer))!=null){
                args.Add(n);
                claimer.Claim(COMMA);
            }
            claimer.Claim(RIGHT_BRACKET);
            return args;
        }

        private static TArgNamer SingleArg(StringClaimer claimer){
            return TArgNamer.Claim(claimer);
        }


        override public Var Parse(Scope scope){
            VarFunction func = null;
            func = new VarFunction(dat => {
                VarList scopeList = new VarList();
                scopeList.parent = func.scope.variables;
                Scope subscope = new Scope(scopeList);
                subscope.escape = func.scope.escape;
                int index = 0;
                for(int i=0; i < args.Count; i++){
                    index = args[i].AppendToScope(index, func.scope, dat, subscope);
                }
                return body.Parse(subscope);
            });
            func.scope = scope;
            func.FunctionText = raw;
            if(name != null){
                name.Set(scope, func);
            }
            return func;
        }
    }

    abstract class TArgNamer : Token{
        new public static TArgNamer Claim(StringClaimer claimer){
            return TArgVariable.Claim(claimer) as TArgNamer;
        }

        public abstract int AppendToScope(int index, Scope called, CallData callData, Scope scopetarget);
    }

    class TArgVariable : TArgNamer{
        TIdentifier var;

        new public static TArgVariable Claim(StringClaimer claimer){
            TIdentifier v = TIdentifier.Claim(claimer);
            if(v != null){
                TArgVariable argV = new TArgVariable();
                v.isLocal = true;
                argV.var = v;
                return argV;
            }
            return null;
        }

        override public int AppendToScope(int index, Scope called, CallData callData, Scope scopetarget){
            if(callData.num_args.ContainsKey(index)){
                var.Set(scopetarget, callData.num_args[index]);
            }else{
                var.Set(scopetarget, Var.undefined);
            }
            return ++index;
        }
    }
}