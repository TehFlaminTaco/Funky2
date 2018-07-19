using System.Collections.Generic;
using System.Text;
using System;

namespace Funky{
    class Meta{
        private static VarList meta;

        public static VarList GetMeta(){
            return meta ?? (meta = GenerateMeta());
        }

        public static VarList GenerateMeta(){
            VarList metas = new VarList();
            metas["list"] = _List();
            metas["string"] = _String();
            metas["number"] = _Number();
            metas["function"] = _Function();

            return metas;
        }

        private static VarList _String(){
            VarList str = new VarList();

            str["tobool"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asString().data.Length));

            str["lt[side=left,left=string,right=string]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asString().data.CompareTo(dat.num_args[1].asString().data) == -1 ? 1 : 0));
            str["le[side=left,left=string,right=string]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asString().data.CompareTo(dat.num_args[1].asString().data) <= 0 ? 1 : 0));
            str["gt[side=left,left=string,right=string]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asString().data.CompareTo(dat.num_args[1].asString().data) == 1 ? 1 : 0));
            str["ge[side=left,left=string,right=string]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asString().data.CompareTo(dat.num_args[1].asString().data) >= 0 ? 1 : 0));
            str["eq[side=left,left=string,right=string]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asString().data == dat.num_args[1].asString().data ? 1 : 0));
            str["ne[side=left,left=string,right=string]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asString().data != dat.num_args[1].asString().data ? 1 : 0));
            str["eq"] = new VarFunction(dat => new VarNumber(0));
            str["ne"] = new VarFunction(dat => new VarNumber(1));

            str["add"] = new VarFunction(dat => new VarString(dat.num_args[0].asString().data + dat.num_args[1].asString().data));
            str["concat"] = new VarFunction(dat => new VarString(dat.num_args[0].asString().data + dat.num_args[1].asString().data));

            return str;
        }

        private static VarList _Function(){
            VarList fnc = new VarList();

            fnc["tobool"] = new VarFunction(dat => new VarNumber(1));
            fnc["tostring"] = new VarFunction(dat => dat.num_args[0].asFunction().FunctionText);

            fnc["eq"] = new VarFunction(dat => new VarNumber(dat.num_args[0] == dat.num_args[1] ? 1 : 0));
            fnc["ne"] = new VarFunction(dat => new VarNumber(dat.num_args[0] == dat.num_args[1] ? 0 : 1));

            return fnc;
        }

        private static VarList _List(){
            VarList lst = new VarList();

            lst["tobool"] = new VarFunction(dat => new VarNumber(1));
            lst["tostring"] = new VarFunction(dat => {
                VarList l = dat.num_args[0].asList();
                StringBuilder sb = new StringBuilder();
                string joiner = "";
                sb.Append("[");
                
                int largest = -1;
                for(int i=0; l.double_vars.ContainsKey(i); i++){
                    sb.Append(joiner);
                    sb.Append(l.double_vars[i].asString());
                    joiner = ", ";
                    largest = i;
                }
                foreach(KeyValuePair<double, Var> kv in l.double_vars){
                    if(kv.Key >= 0 && kv.Key <= largest){
                        continue;
                    }
                    sb.Append(joiner);
                    sb.Append(kv.Key);
                    sb.Append("=");
                    sb.Append(kv.Value.asString());
                    joiner = ", ";
                }
                foreach(KeyValuePair<string, Var> kv in l.string_vars){
                    sb.Append(joiner);
                    sb.Append(kv.Key);
                    sb.Append("=");
                    sb.Append(kv.Value.asString());
                    joiner = ", ";
                }
                foreach(KeyValuePair<Var, Var> kv in l.other_vars){
                    sb.Append(joiner);
                    sb.Append(kv.Key.asString());
                    sb.Append("=");
                    sb.Append(kv.Value.asString());
                    joiner = ", ";
                }

                sb.Append("]");
                return new VarString(sb.ToString());
            });

            lst["eq"] = new VarFunction(dat => new VarNumber(dat.num_args[0] == dat.num_args[1] ? 1 : 0));
            lst["ne"] = new VarFunction(dat => new VarNumber(dat.num_args[0] == dat.num_args[1] ? 0 : 1));

            return lst;
        }

        private static VarList _Number(){
            VarList num = new VarList();

            num["tobool"] = new VarFunction(dat => dat.num_args[0]);

            num["add[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber() + dat.num_args[1].asNumber()));
            num["sub[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber() - dat.num_args[1].asNumber()));
            num["mult[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber() * dat.num_args[1].asNumber()));
            num["div[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber() / dat.num_args[1].asNumber()));
            num["intdiv[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)(dat.num_args[0].asNumber() / dat.num_args[1].asNumber())));
            num["pow[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(Math.Pow(dat.num_args[0].asNumber(), dat.num_args[1].asNumber())));
            num["mod[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber() % dat.num_args[1].asNumber()));
            num["concat[side=left]"] = new VarFunction(dat => new VarString(dat.num_args[0].asString() + dat.num_args[1].asString()));
            num["bitor[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)dat.num_args[0].asNumber().value | (int)dat.num_args[1].asNumber().value));
            num["bitand[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)dat.num_args[0].asNumber().value & (int)dat.num_args[1].asNumber().value));
            num["bitxor[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)dat.num_args[0].asNumber().value ^ (int)dat.num_args[1].asNumber().value));
            num["bitshiftl[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)dat.num_args[0].asNumber().value << (int)dat.num_args[1].asNumber().value));
            num["bitshiftr[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)dat.num_args[0].asNumber().value >> (int)dat.num_args[1].asNumber().value));
            num["lt[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber().value < dat.num_args[1].asNumber().value ? 1 : 0));
            num["le[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber().value <= dat.num_args[1].asNumber().value ? 1 : 0));
            num["gt[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber().value > dat.num_args[1].asNumber().value ? 1 : 0));
            num["ge[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber().value >= dat.num_args[1].asNumber().value ? 1 : 0));
            num["eq[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber().value == dat.num_args[1].asNumber().value ? 1 : 0));
            num["ne[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.num_args[0].asNumber().value != dat.num_args[1].asNumber().value ? 1 : 0));
            num["eq"] = new VarFunction(dat => new VarNumber(0));
            num["ne"] = new VarFunction(dat => new VarNumber(1));


            num["tostring"] = new VarFunction(dat => {
                return new VarString((dat.num_args[0] as VarNumber).value.ToString());
            });

            return num;
        }

        private static string MakeOptions(string[] options, bool[] toggled){
            StringBuilder sb = new StringBuilder();
            string flooper = string.Empty;
            for(int i=0; i < options.Length; i++){
                if(toggled[i]){
                    sb.Append(flooper);
                    sb.Append(options[i]);
                    flooper = ",";
                }
            }

            return sb.ToString();
        }

        private static bool Flop(bool[] list){
            return Flop(list, 0);
        }
        private static bool Flop(bool[] list, int pos){
            if(list[pos] == true){
                list[pos] = false;
                return false;
            }else{
                list[pos] = true;
                if(pos + 1 < list.Length){
                    return Flop(list, pos + 1);
                }else{
                    return true;
                }
            }
        }

        public static Var LR_Get(Var l, Var r, string name, params string[] options){
            bool lValue = l.meta != null;
            bool rValue = r.meta != null;
            string[] newOptions = new string[options.Length+1];
            for(int i = 0; i < options.Length; i++){
                newOptions[i+1] = options[i];
            }
            newOptions[0] = "side=left";
            Var lMeta = Get(l, name, newOptions);
            newOptions[0] = "side=right";
            if(lMeta == null || (!lValue && rValue))
                return Get(r, name, newOptions) ?? lMeta;
            return lMeta;
        }

        public static Var Get(Var val, string name, params string[] options){
            VarList var_meta;
            if(val.meta != null){
                var_meta = val.meta;
            }else{
                if(meta == null)
                    return null;
                if(!meta.string_vars.ContainsKey(val.type)){
                    return null;
                }
                var_meta = (VarList)meta.string_vars[val.type];
            }
            bool[] opUse = new bool[options.Length];
            for(int i=0; i < opUse.Length; i++){
                opUse[i] = true;
            }

            while(opUse.Length > 0){
                string check_name = $"{name}[{MakeOptions(options, opUse)}]";
                if(var_meta.string_vars.ContainsKey(check_name))
                    return var_meta.string_vars[check_name];
                
                if(Flop(opUse))
                    break;
            }
            if(var_meta.string_vars.ContainsKey(name))
                return var_meta.string_vars[name];
            return null;
        }
    }
}