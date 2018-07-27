using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using Funky.Tokens.Literal;
namespace Funky.Tokens{
    class TCall : TLeftExpression{
        TExpression caller;
        List<TArgument> arguments = new List<TArgument>();
        TExpression special_call;

        private static Regex LEFT_BRACKET = new Regex("^\\(");
        private static Regex RIGHT_BRACKET = new Regex("^\\)");
        private static Regex COMMA = new Regex("^,");
        

        override public TExpression GetLeft(){
            return caller;
        }

        override public void SetLeft(TExpression newLeft){
            caller = newLeft;
        }

        override public int GetPrecedence(){
            return -1;
        }

        override public Associativity GetAssociativity(){
            return Associativity.NA;
        }

        public static TCall ExpressionClaim(StringClaimer claimer, TExpression left){
            TExpression exp = TDeoperator.Claim(claimer) as TExpression ?? TBlock.Claim(claimer) as TExpression ?? TLiteralString.Claim(claimer) as TExpression ?? TLiteralStringTemplate.Claim(claimer);
            if(exp != null){
                TCall newCall = new TCall();
                newCall.caller = left;
                newCall.special_call = exp;
                return newCall;
            }
            return null;
        }

        new public static TCall LeftClaim(StringClaimer claimer, TExpression left){
            Claim lb = claimer.Claim(LEFT_BRACKET);
            if(!lb.success){ // Left Bracket is a requirement.
                return ExpressionClaim(claimer, left);
            }
            lb.Pass(); // At this point, we cannot fail.

            TCall newCall = new TCall();
            newCall.caller = left;

            while(true){
                Claim rb = claimer.Claim(RIGHT_BRACKET);
                if(rb.success){
                    rb.Pass();
                    break;
                }
                TArgument newArg = TArgument.Claim(claimer);
                if(newArg == null){
                    break;
                }
                newCall.arguments.Add(newArg);
                claimer.Claim(COMMA);
            }

            return newCall;
        }

        override public Var Parse(Scope scope){
            VarList argList = new VarList();
            if(special_call != null){
                if(special_call is TBlock){
                    VarFunction func = null;
                    argList.double_vars[0] = func = new VarFunction(dat => special_call.Parse(func.scope));
                    func.scope = scope;
                }else{
                    argList.double_vars[0] = special_call.Parse(scope);
                }
            }else{
                int index = 0;
                for(int i=0; i < arguments.Count; i++){
                    index = arguments[i].AppendArguments(argList, index, scope);
                }
            }
            Var callVar = caller.Parse(scope).asFunction();
            CallData callData = new CallData();
            callData.num_args = argList.double_vars;
            callData.str_args = argList.string_vars;
            callData.var_args = argList.other_vars;
            return callVar.Call(callData);
        }

    }
}