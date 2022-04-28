using System.Collections;
using System.Collections.Generic;
namespace Funky{
    public class VarList : Var {
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
            if(t == Var.undefined){
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
                return double_vars.ContainsKey(n) ? double_vars[n] : Var.undefined;
            }
            if(key is VarString s){
                return string_vars.ContainsKey(s) ? string_vars[s] : Var.undefined;
            }
            return other_vars.ContainsKey(key) ? other_vars[key] : Var.undefined;
        }

        public override Var Set(Var key, Var val){
            Var metaFunc = Meta.Get(this, "set", $"key({key.type})", $"value({val.type})");

            if(!(metaFunc is VarNull))
                return metaFunc.Call(new CallData(this, key, val));
            
            bool assignHere = false;
            if(writeParent == null)
                assignHere = true;
            else if (key is VarString s && defined.Contains(s))
                assignHere = true;
            else if ((ThisGet(key)??Var.undefined) != Var.undefined) // We ?? just incase. Because, /shrug.
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

        public Var ThisSet(Var key, Var val){
            if(key is VarNumber n){
                return double_vars[n] = val;
            }
            if(key is VarString s){
                return string_vars[s] = val;
            }
            return other_vars[key] = val;
        }

        public IEnumerable<KeyValuePair<Var, Var>> AllVars()
        {
            foreach(var kv in double_vars){
                yield return new KeyValuePair<Var, Var>(kv.Key, kv.Value);
            }
            foreach(var kv in string_vars){
                yield return new KeyValuePair<Var, Var>(kv.Key, kv.Value);
            }
            foreach(var kv in other_vars){
                yield return new KeyValuePair<Var, Var>(kv.Key, kv.Value);
            }
        }
    }
}