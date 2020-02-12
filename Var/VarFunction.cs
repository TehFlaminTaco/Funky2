using System.Collections.Generic;
namespace Funky{


    public class VarFunction : Var{
        public System.Func<CallData, Var> action;

        public Scope scope;

        public string FunctionText = "[internal function]";

        public VarFunction(System.Func<CallData, Var> todo) : base(){
            this.action = todo;
        }

        public override Var Call(CallData callData){
            return action(callData);
        }

        public override VarFunction asFunction(){
            return this;
        }
    }


}