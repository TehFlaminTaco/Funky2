using Funky;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
            list["getmeta"] = new VarFunction(dat => {
                Var val = dat.Get(0).Otherwise(Var.nil).Get();
                VarList var_meta;
                if(val.meta != null){
                    var_meta = val.meta;
                }else{
                    VarList meta = Meta.GetMeta();
                    if(meta == null)
                        return Var.nil;
                    if(!meta.string_vars.ContainsKey(val.type)){
                        return Var.nil;
                    }
                    var_meta = (VarList)meta.string_vars[val.type];
                }
                return var_meta;
            });
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
            Random rng = new Random();
            list["sort"] = new VarFunction(dat => {
                VarList sortable = dat.Get(0).Required().GetList();
                Var method = dat.Get(1).Required().Get();
                int length = 0;
                while(sortable.double_vars.ContainsKey(length))length++;
                if(length <= 1){ // A list of length 1 or less is always sorted.
                        return sortable;
                }
                int pivot = rng.Next()%length;
                Var pivotVal = sortable.double_vars[pivot];
                VarList leftList = new VarList();
                VarList rightList = new VarList();
                VarList pivotList = new VarList();
                pivotList[0] = pivotVal;
                int leftListLen = 0;
                int rightListLen = 0;
                int pivotListLen = 1;
                bool allEqual = true;
                for(int i=0; i < length; i++){
                    if(i==pivot)
                        continue;
                    Var curVal = sortable.double_vars[i];
                    int sign = Math.Sign(method.Call(new CallData(curVal, pivotVal)).asNumber());
                    if(sign<0){
                        leftList.double_vars[leftListLen++] = curVal;
                        allEqual = false;
                    }else if(sign>0){
                        rightList.double_vars[rightListLen++] = curVal;
                        allEqual = false;
                    }else{
                        pivotList[pivotListLen++] = curVal;
                    }
                }
                if(allEqual)
                    return pivotList;
                leftList = list["sort"].Call(new CallData(leftList, method)).asList();
                rightList = list["sort"].Call(new CallData(rightList, method)).asList();

                // Needed incase we get given a bad list.
                leftListLen = 0;
                rightListLen = 0;
                if(leftList!=null)while(leftList.double_vars.ContainsKey(leftListLen))leftListLen++;
                if(rightList!=null)while(rightList.double_vars.ContainsKey(rightListLen))rightListLen++;

                VarList outList = new VarList();
                for(int i=0; i<leftListLen; i++){
                    outList.double_vars[i] = leftList.double_vars[i];
                }
                for(int i=0; i<pivotListLen; i++){
                    outList.double_vars[leftListLen+i] = pivotList.double_vars[i];
                }
                for(int i=0; i<rightListLen; i++){
                    outList.double_vars[leftListLen+pivotListLen+i]=rightList.double_vars[i];
                }
                return outList;
            });
            list["map"] = new VarFunction(dat => {
                VarList mappable = dat.Get(0).Required().GetList();
                VarFunction callable = dat.Get(1).Required().Get().asFunction();
                VarList mapped = new VarList();
                foreach(var kv in mappable.AllVars()){
                    var cd = new CallData();
                    cd._num_args[0] = kv.Value;
                    cd._num_args[1] = kv.Key;
                    cd._num_args[2] = mappable;
                    cd._str_args["k"] = cd._str_args["key"] = kv.Key;
                    cd._str_args["v"] = cd._str_args["value"] = kv.Value;
                    cd._str_args["l"] = cd._str_args["list"] = mappable;
                    mapped[kv.Key] = callable.Call(cd);
                }
                return mapped;
            });
            list["fold"] = new VarFunction(dat => {
                VarList foldable = dat.Get(0).Required().GetList();
                VarFunction callable = dat.Get(1).Required().Get().asFunction();
                VarList folded = new VarList();
                if(!foldable.double_vars.ContainsKey(0))return folded;
                Var last = foldable[0];
                for(int i = 1; foldable.double_vars.ContainsKey(i); i++){
                    var cd = new CallData();
                    cd._num_args[0] = last;
                    last = cd._num_args[1] = foldable.double_vars[i];

                    folded[i-1] = callable.Call(cd);
                }
                return folded;
            });
            list["reduce"] = new VarFunction(dat => {
                VarList reducable = dat.Get(0).Required().GetList();
                VarFunction callable = dat.Get(1).Required().Get().asFunction();
                if(!reducable.double_vars.ContainsKey(0))return Var.nil;
                Var last = reducable[0];
                for(int i = 1; reducable.double_vars.ContainsKey(i); i++){
                    var cd = new CallData();
                    cd._num_args[0] = last;
                    cd._num_args[1] = reducable.double_vars[i];

                    last = callable.Call(cd);
                }
                return last;
            });
            list["cumulate"] = new VarFunction(dat => {
                VarList cumulatable = dat.Get(0).Required().GetList();
                VarFunction callable = dat.Get(1).Required().Get().asFunction();
                VarList cumulated = new VarList();
                if(!cumulatable.double_vars.ContainsKey(0))return cumulated;
                Var last = cumulated[0] = cumulatable[0];
                
                for(int i = 1; cumulatable.double_vars.ContainsKey(i); i++){
                    var cd = new CallData();
                    cd._num_args[0] = last;
                    cd._num_args[1] = cumulatable.double_vars[i];

                    last = cumulated[i] = callable.Call(cd);
                }
                return cumulated;
            });
            list["where"] = new VarFunction(dat => {
                VarList searchable = dat.Get(0).Required().GetList();
                Var callable = dat.Get(1).Get();
                VarList searched = new VarList();
                int c = 0;
                for(int i = 0; searchable.double_vars.ContainsKey(i); i++){
                    var cd = new CallData();
                    Var v = searchable.double_vars[i];
                    cd._num_args[0] = v;
                    if(callable is VarNull ? !(v is VarNull) : callable.Call(cd).asBool()) searched[c++] = v;
                }
                return searched;
            });
            VarFunction cloneFunc = null;
            list["clone"] = cloneFunc = new VarFunction(dat => {
                VarList clonable = dat.Get(0).Or("list").Required().GetList();
                bool deepClone = dat.Get(1).Or("deep").Otherwise(0).Get().asBool();
                VarList newList = new VarList();
                newList.meta = clonable.meta;
                foreach(var kv in clonable.AllVars()){
                    if(deepClone && kv.Value is VarList){
                        newList[kv.Key] = cloneFunc.Call(new CallData(kv.Value, 1));
                    }else{
                        newList[kv.Key] = kv.Value;
                    }
                }
                return newList;
            });
            return list;
        }
    }
}