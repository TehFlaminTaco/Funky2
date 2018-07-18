using System.Collections.Generic;
using System.Text;

namespace Funky{
    class Meta{
        public static VarList meta = generateMeta();
        public static VarList generateMeta(){
            VarList metas = new VarList();
            metas["list"] = _List();
            metas["string"] = _String();
            metas["number"] = _Number();

            return metas;
        }

        private static VarList _String(){
            VarList str = new VarList();

            

            return str;
        }

        private static VarList _List(){
            VarList lst = new VarList();

            return lst;
        }

        private static VarList _Number(){
            VarList num = new VarList();

            num["tostring"] = new VarFunction(delegate(CallData dat){
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

        private static bool flop(bool[] list){
            return flop(list, 0);
        }
        private static bool flop(bool[] list, int pos){
            if(list[pos] == true){
                list[pos] = false;
                return false;
            }else{
                list[pos] = true;
                if(pos + 1 < list.Length){
                    return flop(list, pos + 1);
                }else{
                    return true;
                }
            }
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
                
                if(flop(opUse))
                    break;
            }
            if(var_meta.string_vars.ContainsKey(name))
                return var_meta.string_vars[name];
            return null;
        }
    }
}