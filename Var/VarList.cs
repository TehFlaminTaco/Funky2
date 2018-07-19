using System.Collections.Generic;
namespace Funky{
    class VarList : Var{
        public Dictionary<string, Var> string_vars = new Dictionary<string, Var>();
        public List<string> defined = new List<string>();
        public Dictionary<double, Var> double_vars = new Dictionary<double, Var>();
        public Dictionary<Var, Var> other_vars     = new Dictionary<Var, Var>();

        public VarList parent;

        public override VarList asList(){
            return this;
        }

        public override Var Get(Var key){
            return ThisGet(key) ?? (parent != null ? parent.Get(key) : null);
        }

        public Var ThisGet(Var key){
            if(key is VarNumber n){
                return double_vars.ContainsKey(n) ? double_vars[n] : base.Get(key);
            }
            if(key is VarString s){
                return string_vars.ContainsKey(s) ? string_vars[s] : base.Get(key);
            }
            return other_vars.ContainsKey(key) ? other_vars[key] : base.Get(key);
        }

        public override Var Set(Var key, Var value){
            return ThisSet(key, value); // Not sure if I need this..?
        }

        public Var ThisSet(Var key, Var val){
            Var metaFunc = Meta.Get(this, "set", $"key({key.type})", $"value({val.type})");

            if(metaFunc != null)
                return metaFunc.Call(new CallData((VarString)key, val));
            
            bool assignHere = false;
            if(parent == null)
                assignHere = true;
            else if (key is VarString s && defined.Contains(s))
                assignHere = true;
            else if (ThisGet(key) != null)
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
                return parent.Set(key, val);
        }
    }
}