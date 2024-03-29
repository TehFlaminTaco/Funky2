using System.Collections.Generic;
using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Funky{
    class Meta{
        private static VarList meta;

        public static VarList GetMeta(){
            return meta ?? GenerateMeta();
        }

        public static VarList GenerateMeta(){
            meta = new VarList();
            meta["base"] = _Base();

            meta["list"] = _List();
            meta["string"] = _String();
            meta["number"] = _Number();
            meta["function"] = _Function();
            meta["event"] = _Event();
            meta["null"] = _Null();

            return meta;
        }

        private static VarList newMeta(){
            VarList l = new VarList();
            return l;
        }

        private static VarList _Base(){
            VarList bas = newMeta();
            bas["concat"] = new VarFunction(dat => new VarString(dat.Get(0).Required().GetString().data + dat.Get(1).Required().GetString().data));
            bas["eq"] = new VarFunction(dat => dat.Get(0).Get() == dat.Get(1).Get() ? 1 : 0);
            bas["ne"] = new VarFunction(dat => dat.Get(0).Get() == dat.Get(1).Get() ? 0 : 1);

            bas["unp"] = new VarFunction(dat => dat.Get(0).Required().GetNumber());
            bas["unm"] = new VarFunction(dat => -dat.Get(0).Required().GetNumber().value);
            bas["not"] = new VarFunction(dat => dat.Get(0).Required().Get().asBool() ? 0 : 1);
            bas["len"] = new VarFunction(dat => 0);
            return bas;
        }

        private static VarList _Null(){
            VarList nul = _Base();
            nul["tostring"] = new VarFunction(dat => dat._num_args[0] is VarNull n ? n.id : "what? stop");
            nul["tobool"] = new VarFunction(dat => 0);
            nul["not"] = new VarFunction(dat => 1);
            nul["eq[left=null,right=null]"] = new VarFunction(dat => dat.Get(0).Get() == dat.Get(1).Get() ? 1 : 0);

            return nul;
        }

        private static VarList _String(){
            VarList str = _Base();

            str["get"] = new VarFunction(dat => {
                VarString s = dat.Get(0).Required().GetString();
                Var n = dat.Get(1).Required().Get();
                if (n is VarNumber){
                    return (VarString)(""+s.data[(int)(Math.Abs(n.asNumber().value)%s.data.Length)]);
                }else{
                    return Globals.get()["string"].Get(n);
                }
            });

            str["tobool"] = new VarFunction(dat => dat.Get(0).Required().GetString().data.Length);
            str["tonumber"] = new VarFunction(dat => {
                double d = 0;
                Double.TryParse(dat.Get(0).Required().GetString().data, out d);
                return d;
            });
            str["len"] = new VarFunction(dat => dat.Get(0).Required().GetString().data.Length);

            str["lt[side=left,left=string,right=string]"] = new VarFunction(dat => dat.Get(0).Required().GetString().data.CompareTo(dat.Get(1).Required().GetString().data) == -1 ? 1 : 0);
            str["le[side=left,left=string,right=string]"] = new VarFunction(dat => dat.Get(0).Required().GetString().data.CompareTo(dat.Get(1).Required().GetString().data) <= 0 ? 1 : 0);
            str["gt[side=left,left=string,right=string]"] = new VarFunction(dat => dat.Get(0).Required().GetString().data.CompareTo(dat.Get(1).Required().GetString().data) == 1 ? 1 : 0);
            str["ge[side=left,left=string,right=string]"] = new VarFunction(dat => dat.Get(0).Required().GetString().data.CompareTo(dat.Get(1).Required().GetString().data) >= 0 ? 1 : 0);
            str["eq[side=left,left=string,right=string]"] = new VarFunction(dat => dat.Get(0).Required().GetString().data == dat.Get(1).Required().GetString().data ? 1 : 0);
            str["ne[side=left,left=string,right=string]"] = new VarFunction(dat => dat.Get(0).Required().GetString().data != dat.Get(1).Required().GetString().data ? 1 : 0);
            str["eq"] = new VarFunction(dat => 0);
            str["ne"] = new VarFunction(dat => 1);

            str["add"] = new VarFunction(dat => new VarString(dat.Get(0).Required().GetString().data + dat.Get(1).Required().GetString().data));
            str["mult[side=left,left=string,right=number]"] = new VarFunction(dat => {
                double repeatAmt = dat.Get(1).Required().GetNumber().value;
                string builtString = "";
                string baseString = dat.Get(0).Required().GetString().data;
                bool doFlip = false;
                if(repeatAmt < 0d){
                    repeatAmt = -repeatAmt;
                    doFlip = true;
                }
                for(int i = 0; i < repeatAmt; i++)
                    builtString += baseString;
                float remaining = (float)(repeatAmt%1f);
                if(remaining > 0f)
                    builtString += baseString.Substring(0, (int)(baseString.Length * remaining));
                
                if(doFlip){
                    char[] arr = builtString.ToCharArray();
                    Array.Reverse(arr);
                    builtString = new String(arr);
                }
                return new VarString(builtString);
            });
            str["mult[side=right,left=number,right=string]"] = new VarFunction(dat => {
                double repeatAmt = dat.Get(0).Required().GetNumber().value;
                string builtString = "";
                string baseString = dat.Get(1).Required().GetString().data;
                bool doFlip = false;
                if(repeatAmt < 0d){
                    repeatAmt = -repeatAmt;
                    doFlip = true;
                }
                for(int i = 0; i < repeatAmt; i++)
                    builtString += baseString;
                float remaining = (float)(repeatAmt%1f);
                if(remaining > 0f)
                    builtString += baseString.Substring(0, (int)(baseString.Length * remaining));
                
                if(doFlip){
                    char[] arr = builtString.ToCharArray();
                    Array.Reverse(arr);
                    builtString = new String(arr);
                }
                return new VarString(builtString);
            });

            return str;
        }

        private static VarList _Function(){
            VarList fnc = _Base();

            fnc["tobool"] = new VarFunction(dat => new VarNumber(1));
            fnc["tostring"] = new VarFunction(dat => dat.Get(0).Required().GetFunction().FunctionText);

            fnc["eq"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().Get() == dat.Get(1).Get() ? 1 : 0));
            fnc["ne"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().Get() == dat.Get(1).Get() ? 0 : 1));

            return fnc;
        }

        private static VarList _List(){
            VarList lst = _Base();

            lst["tobool"] = new VarFunction(dat => new VarNumber(1));
            lst["tostring"] = new VarFunction(dat => {
                VarList l = dat.Get(0).Required().GetList();
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
                    if(kv.Key >= 0 && kv.Key <= largest && kv.Key%1==0){
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
            lst["len"] = new VarFunction(dat => {
                VarList l = dat.Get(0).Required().GetList();
                int i=0;
                while(l.double_vars.ContainsKey(i) && !(l.double_vars[i] is VarNull))
                    i++;
                return i;
            });

            lst["eq"] = new VarFunction(dat => new VarNumber(dat.Get(0).Get() == dat.Get(1).Get() ? 1 : 0));
            lst["ne"] = new VarFunction(dat => new VarNumber(dat.Get(0).Get() == dat.Get(1).Get() ? 0 : 1));

            lst["get"] = new VarFunction(dat => Globals.get()["list"].asList().ThisGet(dat.Get(1).Required().Get()));

            lst["dispose"] = new VarFunction(dat => {
                var lst = dat.Get(0).Get();
                if(lst["dispose"] is Var dsps && !(dsps is VarNull))return dsps.Call(new CallData());
                if(lst["destroy"] is Var dstry && !(dstry is VarNull))return dstry.Call(new CallData());
                if(lst["close"] is Var cls && !(cls is VarNull))cls.Call(new CallData());
                return Var.nil;
            });

            return lst;
        }

        private static VarList _Number(){
            VarList num = _Base();

            num["tobool"] = new VarFunction(dat => dat.Get(0).Required().GetNumber());

            num["add[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber() + dat.Get(1).Required().GetNumber()));
            num["sub[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber() - dat.Get(1).Required().GetNumber()));
            num["mult[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber() * dat.Get(1).Required().GetNumber()));
            num["div[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber() / dat.Get(1).Required().GetNumber()));
            num["intdiv[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)(dat.Get(0).Required().GetNumber() / dat.Get(1).Required().GetNumber())));
            num["pow[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(Math.Pow(dat.Get(0).Required().GetNumber(), dat.Get(1).Required().GetNumber())));
            num["mod[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber() % dat.Get(1).Required().GetNumber()));
            num["concat[side=left]"] = new VarFunction(dat => new VarString(dat.Get(0).Required().GetString() + dat.Get(1).Required().GetString()));
            num["bitor[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)dat.Get(0).Required().GetNumber().value | (int)dat.Get(1).Required().GetNumber().value));
            num["bitand[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)dat.Get(0).Required().GetNumber().value & (int)dat.Get(1).Required().GetNumber().value));
            num["bitxor[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)dat.Get(0).Required().GetNumber().value ^ (int)dat.Get(1).Required().GetNumber().value));
            num["bitshiftl[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)dat.Get(0).Required().GetNumber().value << (int)dat.Get(1).Required().GetNumber().value));
            num["bitshiftr[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber((int)dat.Get(0).Required().GetNumber().value >> (int)dat.Get(1).Required().GetNumber().value));
            num["lt[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber().value < dat.Get(1).Required().GetNumber().value ? 1 : 0));
            num["le[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber().value <= dat.Get(1).Required().GetNumber().value ? 1 : 0));
            num["gt[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber().value > dat.Get(1).Required().GetNumber().value ? 1 : 0));
            num["ge[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber().value >= dat.Get(1).Required().GetNumber().value ? 1 : 0));
            num["eq[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber().value == dat.Get(1).Required().GetNumber().value ? 1 : 0));
            num["ne[side=left,left=number,right=number]"] = new VarFunction(dat => new VarNumber(dat.Get(0).Required().GetNumber().value != dat.Get(1).Required().GetNumber().value ? 1 : 0));
            num["eq"] = new VarFunction(dat => new VarNumber(0));
            num["ne"] = new VarFunction(dat => new VarNumber(1));


            num["tostring"] = new VarFunction(dat => {
                return new VarString(dat.Get(0).Required().GetNumber().value.ToString());
            });

            return num;
        }

        private static VarList _Event(){
            VarList evt = _Base();
            evt["tostring"] = new VarFunction(dat => $"Event({dat.Get(0).Required().GetEvent().name})");
            evt["tobool"] = new VarFunction(dat => 1);
            evt["eq[left=null,right=null]"] = new VarFunction(dat => dat.Get(0).Get() == dat.Get(1).Get() ? 1 : 0);
            evt["get"] = new VarFunction(dat => Globals.get()["event"].Get(dat.Get(1).Required().Get()));

            return evt;
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

        public static VarList GetMetaTable(Var val){
            if(val.meta != null){
                return val.meta;
            }else{
                if(meta == null)
                    return null;
                if(!meta.string_vars.ContainsKey(val.type)){
                    return null;
                }
                return (VarList)meta.string_vars[val.type];
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
            if(lMeta is VarNull || (!lValue && rValue)){
                Var rMeta = Get(r, name, newOptions);
                if(rMeta is VarNull){
                    return lMeta;
                }
                return rMeta;
            }
            return lMeta;
        }


        private static Regex keyMatcher = new Regex(",?(\\w+=\\w+)");
        public static Var Get(Var val, string name, params string[] options){
            StringBuilder nameChunk = new StringBuilder();
            nameChunk.Append(name);
            nameChunk.Append('[');
            string metaMatch = nameChunk.ToString();

            VarList var_meta = GetMetaTable(val);
            if(var_meta == null)
                return Var.nil;
            int longestMatch = 0;
            Var longestFunc = Var.nil;
            foreach(var metaFunction in var_meta.string_vars){
                string key = metaFunction.Key;
                if(key == name && longestMatch == 0){
                    longestMatch = 1;
                    longestFunc = metaFunction.Value;
                }
                if(key.StartsWith(metaMatch)){
                    var subkeys = keyMatcher.Matches(key.Substring(metaMatch.Length));
                    if(subkeys.Count <= longestMatch)
                        continue;
                    bool validFunc = true;
                    foreach(Match match in subkeys){
                        if(!options.Contains(match.Groups[1].Value)){
                            validFunc = false; break;
                        }
                    }
                    if(!validFunc)
                        continue;
                    longestMatch = subkeys.Count;
                    longestFunc = metaFunction.Value;
                }
            }
            return longestFunc;
        }
    }
}