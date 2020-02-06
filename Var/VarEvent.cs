using System.Collections.Generic;
namespace Funky{
    public class VarEvent : Var{
        List<Var> callbacks = new List<Var>();

        public string name = "EMPTY";

        public VarEvent(){}
        public VarEvent(string name){
            this.name = name;
        }

        public override VarEvent asEvent(){
            return this;
        }

        public VarEvent Hook(Var action){
            callbacks.Add(action);
            return this;
        }

        public VarEvent Hook(System.Func<CallData, Var> todo){
            callbacks.Add(new VarFunction(todo));
            return this;
        }
        
        public VarEvent Unhook(Var action){
            callbacks.Remove(action);
            return this;
        }

        public override Var Call(CallData unusedCD){
            Var lastRet = Var.nil;
            foreach(Var action in callbacks){
                CallData cd = new CallData();
                cd.num_args = new Dictionary<double, Var>();
                cd.str_args = new Dictionary<string, Var>();
                cd.var_args = new Dictionary<Var,    Var>();
                Var thisRet = action.Call(cd);
                if(thisRet != Var.nil)
                    lastRet = thisRet;
            }
            return lastRet;
        }
    }
}