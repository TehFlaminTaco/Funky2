using System;
using System.Collections.Generic;

namespace Funky{
    public class CallData{
        public Dictionary<double, Var> _num_args;
        public Dictionary<string, Var> _str_args;
        public Dictionary<Var, Var>    _var_args;

        public CallData(params Var[] args){
            _num_args = new Dictionary<double, Var>();
            _str_args = new Dictionary<string, Var>();
            _var_args = new Dictionary<Var,    Var>();
            for(int i=0; i < args.Length; i++){
                _num_args[i] = args[i];
            }
        }
        public CallData(){
            _num_args = new Dictionary<double, Var>();
            _str_args = new Dictionary<string, Var>();
            _var_args = new Dictionary<Var,    Var>();
        }

        public ArgumentGetter Get(double n){
            return new ArgumentGetter(this).Or(n);
        }
        public ArgumentGetter Get(string s){
            return new ArgumentGetter(this).Or(s);
        }
    }

    public class ArgumentGetter{
        List<string> argumentNames = new List<string>();
        double argumentIndex = Int32.MinValue;
        ArgType argType = ArgType.Any;
        Var defResult = null;
        Exception onFail = null;
        CallData holder;

        public ArgumentGetter(CallData dat){
            holder = dat;
        }

        public ArgumentGetter Or(double key){
            if(argumentIndex != Int32.MinValue)
                throw new InvalidOperationException("Argument index already set.");
            argumentIndex = key;
            return this;
        }
        public ArgumentGetter Or(string key){
            argumentNames.Add(key);
            return this;
        }
        public ArgumentGetter As(ArgType typ){
            argType = typ;
            return this;
        }

        public ArgumentGetter Otherwise(Var def){
            if(onFail != null)
                throw new InvalidOperationException("Default value defined when an on fail exception was defined, thus invalidating it.");
            if(defResult != null)
                throw new InvalidOperationException("Default value already defined.");
            defResult = def;
            return this;
        }
        
        public ArgumentGetter Otherwise(Exception e){
            if(defResult != null)
                throw new InvalidOperationException("Exception defined when a Default value is already defined, thus can never be called.");
            if(onFail != null)
                throw new InvalidOperationException("OnFail exception already defined.");
            onFail = e;
            return this;
        }

        public ArgumentGetter Required(){
            string errorMessage = "Invalid argument ";
            if(argumentIndex != Int32.MinValue){
                errorMessage += argumentIndex;
                if(argumentNames.Count > 0)
                    errorMessage += ":" + argumentNames[0];
            }else if(argumentNames.Count > 0){
                errorMessage += argumentNames[0];
            }else{
                errorMessage += "unknown";
            }
            switch(argType){
                case ArgType.Number:
                    errorMessage += ", expected number.";
                    break;
                case ArgType.String:
                    errorMessage += ", expected string.";
                    break;
                case ArgType.List:
                    errorMessage += ", expected list.";
                    break;
                case ArgType.Function:
                    errorMessage += ", expected function.";
                    break;
                case ArgType.Event:
                    errorMessage += ", expected event.";
                    break;
                default:
                    errorMessage += ", expected a value.";
                    break;
            }
            return this.Otherwise(new FunkyArgumentException(errorMessage));
        }

        public Var Get(){
            foreach(String s in argumentNames){
                if(holder._str_args.ContainsKey(s)){
                    Var o = holder._str_args[s];
                    if((GetArgType(o)&argType)!=ArgType.UNKNOWN)
                        return o;
                }
            }
            if(argumentIndex != Int32.MinValue && holder._num_args.ContainsKey(argumentIndex)){
                Var o = holder._num_args[argumentIndex];
                if((GetArgType(o)&argType)!=ArgType.UNKNOWN)
                    return o;
            }
            if(onFail != null)
                throw onFail;
            return defResult??Var.nil;
        }

        public VarString GetString(){
            return Get().asString();
        }
        public VarNumber GetNumber(){
            return Get().asNumber();
        }
        public VarList GetList(){
            return Get().asList();
        }
        public VarFunction GetFunction(){
            return Get().asFunction();
        }
        public VarEvent GetEvent(){
            return Get().asEvent();
        }

        private static ArgType GetArgType(Var v){
            if (v is VarNumber)
                return ArgType.Number;
            if (v is VarString)
                return ArgType.String;
            if (v is VarList)
                return ArgType.List;
            if (v is VarFunction)
                return ArgType.Function;
            if (v is VarEvent)
                return ArgType.Event;
            return ArgType.UNKNOWN;
        }
    }

    public enum ArgType{
        UNKNOWN = 0,
        Number = 1,
        String = 2,
        List = 4,
        Function = 8,
        Event = 16,
        Any = 31
    }
}