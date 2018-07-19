using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
namespace Funky.Tokens{
    class TCall : TLeftExpression{
        TExpression caller;
        List<TArgument> arguments = new List<TArgument>();

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

        new public static TCall LeftClaim(StringClaimer claimer, TExpression left){
            Claim lb = claimer.Claim(LEFT_BRACKET);
            if(!lb.success){ // Left Bracket is a requirement.
                return null;
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
            Scope hackedScope = new Scope();
            VarList argList = new VarList();
            hackedScope.variables = argList;
            argList.parent = scope.variables;
            int index = 0;
            for(int i=0; i < arguments.Count; i++){
                index = arguments[i].AppendArguments(argList, index, hackedScope);
            }
            Var callVar = caller.Parse(scope);
            if(callVar == null){
                return null;
            }
            CallData callData = new CallData();
            callData.num_args = argList.double_vars;
            callData.str_args = argList.string_vars;
            callData.var_args = argList.other_vars;
            return callVar.Call(callData);
        }

    }

    abstract class TArgument : TExpression{
        public abstract int AppendArguments(VarList argumentList, int index, Scope scope);
        override public Var Parse(Scope scope){return null;} // Never parse. Never.

        new public static TArgument Claim(StringClaimer claimer){
            return TArgExpression.Claim(claimer);
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
}