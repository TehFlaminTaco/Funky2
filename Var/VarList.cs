using System.Collections.Generic;
namespace Funky{
    public class VarList : Var{
        public Dictionary<string, Var> string_vars = new Dictionary<string, Var>();
        public List<string> defined = new List<string>();
        public Dictionary<double, Var> double_vars = new Dictionary<double, Var>();
        public Dictionary<Var, Var> other_vars     = new Dictionary<Var, Var>();

        public VarList readParent;
        public VarList writeParent;
        public VarList parent {
            get{
                return readParent ?? writeParent;
            }
            set{
                readParent = value;
                writeParent = value;
            }
        }

        public override VarList asList(){
            return this;
        }

        public override Var Get(Var key){
            Var t = ThisGet(key);
            if(t == Var.nil){
                if (readParent != null){
                    Var parVal = readParent.Get(key);
                    if(!(parVal is VarNull))
                        return parVal;
                }
                return base.Get(key);
            }
            return t;
        }

        public Var ThisGet(Var key){
            if(key is VarNumber n){
                return double_vars.ContainsKey(n) ? double_vars[n] : Var.nil;
            }
            if(key is VarString s){
                return string_vars.ContainsKey(s) ? string_vars[s] : Var.nil;
            }
            return other_vars.ContainsKey(key) ? other_vars[key] : Var.nil;
        }

        public override Var Set(Var key, Var value){
            return ThisSet(key, value); // Not sure if I need this..?
        }

        public Var ThisSet(Var key, Var val){
            Var metaFunc = Meta.Get(this, "set", $"key({key.type})", $"value({val.type})");

            if(!(metaFunc is VarNull))
                return metaFunc.Call(new CallData(this, (VarString)key, val));
            
            bool assignHere = false;
            if(writeParent == null)
                assignHere = true;
            else if (key is VarString s && defined.Contains(s))
                assignHere = true;
            else if ((ThisGet(key)??Var.nil) != Var.nil) // We ?? just incase. Because, /shrug.
                assignHere = true;
            
            if(assignHere){
                if(key is VarNumber n){
                    return double_vars[n] = val;
                }
                if(key is VarString s){
                    return string_vars[s] = val;
                }
                return other_vars[key] = val;
            }else
                return writeParent.Set(key, val);
        }
    }
}