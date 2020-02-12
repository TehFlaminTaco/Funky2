using Funky;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace Funky.Libs{
    public static class LibList{
        private static VarList duplicateList(VarList vl){
            VarList nList = new VarList();
            foreach(var kv in vl.double_vars)
                nList[kv.Key] = kv.Value;
            foreach(var kv in vl.string_vars)
                nList[kv.Key] = kv.Value;
            foreach(var kv in vl.other_vars)
                nList[kv.Key] = kv.Value;
            return nList;
        }

        public static VarList Generate(){
            VarList list = new VarList();

            list["setmeta"] = new VarFunction(dat => {
                dat.Get(0).Required().Get().meta = dat.Get(1).Required().Get()?.asList();
                return dat.Get(0).Required().Get();
            });
            list["getmeta"] = new VarFunction(dat => (Var)dat.Get(0).Required().Get().meta??Var.nil);
            list["rawget"] = new VarFunction(dat => {
                VarList l = dat.Get(0).Required().GetList();
                return l.ThisGet(dat.Get(1).Required().Get());
            });
            list["rawset"] = new VarFunction(dat => {
                VarList l = dat.Get(0).Required().GetList();
                return l.ThisSet(dat.Get(1).Required().Get(), dat.Get(2).Required().Get());
            });
            list["apply"] = new VarFunction(dat => {
                VarList lst = dat.Get(0).Required().GetList();
                VarFunction fnc = dat.Get(1).Required().GetFunction();
                CallData cd = new CallData();
                foreach(var kv in lst.double_vars)
                    cd._num_args[kv.Key] = kv.Value;
                foreach(var kv in lst.string_vars)
                    cd._str_args[kv.Key] = kv.Value;
                foreach(var kv in lst.other_vars)
                    cd._var_args[kv.Key] = kv.Value;
                
                return fnc.Call(cd);
            });
            list["reverse"] = new VarFunction(dat => {
                VarList vl = dat.Get(0).Required().GetList();
                VarList nList = duplicateList(vl);
                int l = 0;
                while(vl.double_vars.ContainsKey(l) && !(vl.double_vars[l] is VarNull))
                    l++;
                for(int i=0; i < l; i++)
                    nList[i] = vl[(l-1)-i];
                return nList;
            });
            list["enqueue"] = list["push"] = list["insert"] = new VarFunction(dat => {
                VarList vl = dat.Get(0).Required().GetList();
                Var index = dat.Get(1).Required().Get();
                Var value;
                if(dat._num_args.ContainsKey(2)){
                    value = dat.Get(2).Required().Get();
                    index = (int)Math.Max(index.asNumber().value, 0d);
                }else{
                    value = index;
                    index = 0d;
                }
                int l = 0;
                while(vl.double_vars.ContainsKey(l) && !(vl.double_vars[l] is VarNull))l++;
                for(int i=l-1; i>=index.asNumber().value; i--){
                    vl[i+1] = vl[i];
                }
                vl[index] = value;
                return vl;
            });
            list["pop"] = list["remove"] = new VarFunction(dat => {
                VarList vl = dat.Get(0).Required().GetList();
                Var index = 0;
                if(dat._num_args.ContainsKey(1)){
                    index = (int)Math.Max(dat.Get(1).Required().GetNumber().value, 0d);
                }
                Var ret = vl.double_vars.ContainsKey((int)index.asNumber().value) ? vl.double_vars[(int)index.asNumber().value] : Var.nil;
                for(int i=(int)index.asNumber().value; vl.double_vars.ContainsKey(i); i++){
                    if(vl.double_vars.ContainsKey(i+1))
                        vl.double_vars[i] = vl.double_vars[i+1];
                    else
                        vl.double_vars.Remove(i);
                }
                return ret;
            });
            list["dequeue"] = new VarFunction(dat => {
                VarList vl = dat.Get(0).Required().GetList();
                Var index = 0;
                if(dat._num_args.ContainsKey(1)){
                    index = (int)Math.Max(dat.Get(1).Required().GetNumber().value, 0d);
                }else{
                    for(int l=0;vl.double_vars.ContainsKey(l);l++)
                        index = l;
                }
                Var ret = vl.double_vars.ContainsKey((int)index.asNumber().value) ? vl.double_vars[(int)index.asNumber().value] : Var.nil;
                for(int i=(int)index.asNumber().value; vl.double_vars.ContainsKey(i); i++){
                    if(vl.double_vars.ContainsKey(i+1))
                        vl.double_vars[i] = vl.double_vars[i+1];
                    else
                        vl.double_vars.Remove(i);
                }
                return ret;
            });
            return list;
        }
    }
}