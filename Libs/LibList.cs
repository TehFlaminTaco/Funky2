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
                dat.num_args[0].meta = dat.num_args[1]?.asList();
                return dat.num_args[0];
            });
            list["getmeta"] = new VarFunction(dat => (Var)dat.num_args[0].meta??Var.nil);
            list["rawget"] = new VarFunction(dat => {
                VarList l = dat.num_args[0].asList();
                return l.ThisGet(dat.num_args[1]);
            });
            list["rawset"] = new VarFunction(dat => {
                VarList l = dat.num_args[0].asList();
                return l.ThisSet(dat.num_args[1], dat.num_args[2]);
            });
            list["apply"] = new VarFunction(dat => {
                VarList lst = dat.num_args[0].asList();
                VarFunction fnc = dat.num_args[1].asFunction();
                CallData cd = new CallData();
                cd.num_args = new Dictionary<double, Var>();
                cd.str_args = new Dictionary<string, Var>();
                cd.var_args = new Dictionary<Var,    Var>();

                foreach(var kv in lst.double_vars)
                    cd.num_args[kv.Key] = kv.Value;
                foreach(var kv in lst.string_vars)
                    cd.str_args[kv.Key] = kv.Value;
                foreach(var kv in lst.other_vars)
                    cd.var_args[kv.Key] = kv.Value;
                
                return fnc.Call(cd);
            });
            list["reverse"] = new VarFunction(dat => {
                VarList vl = dat.num_args[0].asList();
                VarList nList = duplicateList(vl);
                int l = 0;
                while(vl.double_vars.ContainsKey(l) && !(vl.double_vars[l] is VarNull))
                    l++;
                for(int i=0; i < l; i++)
                    nList[i] = vl[(l-1)-i];
                return nList;
            });
            list["enqueue"] = list["push"] = list["insert"] = new VarFunction(dat => {
                VarList vl = dat.num_args[0].asList();
                Var index = dat.num_args[1];
                Var value;
                if(dat.num_args.ContainsKey(2)){
                    value = dat.num_args[2];
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
                VarList vl = dat.num_args[0].asList();
                Var index = 0;
                if(dat.num_args.ContainsKey(1)){
                    index = (int)Math.Max(dat.num_args[1].asNumber().value, 0d);
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
                VarList vl = dat.num_args[0].asList();
                Var index = 0;
                if(dat.num_args.ContainsKey(1)){
                    index = (int)Math.Max(dat.num_args[1].asNumber().value, 0d);
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