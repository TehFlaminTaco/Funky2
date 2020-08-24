using System.Collections.Generic;
using System.Linq;
namespace Funky{
    public class Var{
        public string type;
        public VarList meta;

        public static VarNull nil = new VarNull("nil");
        public static VarNull undefined = new VarNull("undefined");
        

        // Sometimes you want a string representation of the type, but you want to do it Programatically.
        // AKA: Making real coders cry.
        public Var(){type = this.GetType().Name.Substring(3).ToLower();}

        public virtual Var Call(CallData callData){
            List<Var> numberArguments = new List<Var>();
            for(int i=0; callData._num_args.ContainsKey(i); i++){
                numberArguments.Add(callData._num_args[i]);
            }
            string[] options = new string[numberArguments.Count];
            for(int i=0; i < options.Length; i++){
                options[i] = $"{i}({numberArguments[i].type})";
            }
            Var callFunc = Meta.Get(this, "call", options);
            if(!(callFunc is VarNull)){
                var newCD = new CallData();
                Dictionary<double, Var> newNumArguments = new Dictionary<double, Var>();
                foreach(var kv in callData._num_args)
                    newNumArguments[kv.Key] = kv.Value;
                newNumArguments[0] = this;
                for(int i=0; i < numberArguments.Count; i++)
                    newNumArguments[i+1] = numberArguments[i];

                newCD._num_args = newNumArguments;
                newCD._str_args = callData._str_args.ToDictionary(e => e.Key, e => e.Value);
                newCD._str_args["this"] = this;
                newCD._var_args = callData._var_args.ToDictionary(e => e.Key, e => e.Value);
                return callFunc.Call(newCD);
            }
            return this;
        }

        public virtual Var TryCall(CallData callData){
            try{
                return Call(callData);
            }catch(System.Exception e){
                System.Console.Error.WriteLine($"ERROR:\n{e.Message}");
            }
            return Var.nil;
        }

        public virtual Var Get(Var key){
            Var callFunc = Meta.Get(this, "get", $"key({key.type})");
            if(!(callFunc is VarNull))
                return callFunc.Call(new CallData(this, key));
            return nil;
        }

        public virtual Var Set(Var key, Var val){
            Var callFunc = Meta.Get(this, "set", $"key({key.type})", $"value({val.type})");
            if(!(callFunc is VarNull))
                return callFunc.Call(new CallData(this, key, val));
            return val;
        }

        public Var this[string key]{
            get{return Get((VarString)key);}
            set{Set((VarString)key, value);}
        }

        public Var this[double key]{
            get{return Get((VarNumber)key);}
            set{Set((VarNumber)key, value);}
        }

        public Var this[Var key]{
            get{return Get(key);}
            set{Set(key, value);}
        }

        public virtual VarString asString(){
            Var callFunc = Meta.Get(this, "tostring");
            if(!(callFunc is VarNull)){
                Var outp = callFunc.Call(new CallData(this));
                if(!(outp is VarString)){
                    return outp.asString();
                }
                return outp as VarString;
            }
            return new VarString("");
        }
        public virtual VarNumber asNumber(){
            Var callFunc = Meta.Get(this, "tonumber");
            if(!(callFunc is VarNull)){
                Var outp = callFunc.Call(new CallData(this));
                if(!(outp is VarNumber)){
                    return outp.asNumber();
                }
                return outp as VarNumber;
            }
            return new VarNumber(0);
        }
        public virtual VarList asList(){
            Var callFunc = Meta.Get(this, "tolist");
            if(!(callFunc is VarNull)){
                Var outp = callFunc.Call(new CallData(this));
                if(!(outp is VarList)){
                    return outp.asList();
                }
                return outp as VarList;
            }
            VarList n = new VarList();
            n.double_vars[0] = this;
            return n;
        }

        public virtual VarFunction asFunction(){
            Var callFunc = Meta.Get(this, "tofunction");
            if(!(callFunc is VarNull)){
                Var outp = callFunc.Call(new CallData(this));
                if(!(outp is VarFunction)){
                    return outp.asFunction();
                }
                return outp as VarFunction;
            }
            return new VarFunction(dat => {
                return this.Call(dat);
            });
        }

        public virtual bool asBool(){
            Var callFunc = Meta.Get(this, "tobool");
            if(!(callFunc is VarNull)){
                Var outp = callFunc.Call(new CallData(this));
                if(!(outp is VarNumber n)){
                    return outp.asBool();
                }
                return n.value != 0;
            }
            return false;
        }

        public virtual VarEvent asEvent(){
            Var callFunc = Meta.Get(this, "toevent");
            if(!(callFunc is VarNull)){
                Var outp = callFunc.Call(new CallData(this));
                if(!(outp is VarEvent)){
                    return outp.asEvent();
                }
                return outp as VarEvent;
            }
            return new VarEvent();
        }

        public static implicit operator Var(string v){
            return new VarString(v);
        }

        public static implicit operator Var(double v){
            return new VarNumber(v);
        }
    }

    public class VarNumber : Var{
        public double value;

        public static implicit operator double(VarNumber var){
            return var.value;
        }
        public VarNumber(double v) : base(){
            value = v;
        }

        public override VarNumber asNumber(){
            return this;
        }
        public override bool asBool(){
            return this!=0;
        }
    }

    public class VarString : Var {
        public string data;
        public static implicit operator string(VarString var){
            return var.data;
        }

        public VarString(string d) : base(){
            data = d;
        }

        public override VarString asString(){
            return this;
        }
    }

    public class VarNull : Var {
        public string id;
        public VarNull(string id){
            this.id = id;
        }
    }
}