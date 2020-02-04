using Funky;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace Funky.Libs{
    public static class LibString{
        public static VarList Generate(){
            VarList str = new VarList();

            str["sub"] = new VarFunction(dat => {
                VarString s = dat.num_args[0].asString();
                VarNumber a = dat.num_args[1].asNumber();
                if(dat.num_args.ContainsKey(2)){
                    return s.data.Substring((int) a, (int) dat.num_args[2].asNumber());
                }else{
                    return s.data.Substring((int) a);
                }
            });
            str["match"] = new VarFunction(dat => {
                string haystack = dat.num_args[0].asString();
                string needle = dat.num_args[1].asString();
                Regex matcher = new Regex(needle);
                Match m = matcher.Match(haystack);
                if(m.Success){
                    if(m.Groups.Count == 1)
                        return m.Groups[0].Value;
                    else{
                        VarList list = new VarList();
                        for(int i=0; i < m.Groups.Count; i++)
                            list[m.Groups[i].Name] = list[i] = m.Groups[i].Value;
                        return list;
                    }
                }
                return Var.nil;
            });
            str["gmatch"] = new VarFunction(dat => {
                string haystack = dat.num_args[0].asString();
                string needle = dat.num_args[1].asString();
                Regex matcher = new Regex(needle);
                MatchCollection M = matcher.Matches(haystack);
                int i = 0;
                return new VarFunction(dat2 => {
                    if(i < M.Count){
                        Match m = M[i++];
                        if(m.Groups.Count == 1)
                            return m.Groups[0].Value;
                        else{
                            VarList list = new VarList();
                            for(int c=0; c < m.Groups.Count; c++)
                                list[m.Groups[c].Name] = list[c] = m.Groups[c].Value;
                            return list;
                        }
                    }
                    return Var.nil;
                });
            });
            string groupFinder = @"^(?:(?<!\\)(?:\\\\)*\(.*?){3}((?:.|\\\)|\\\\)*?)\)";
            str["gsub"] = new VarFunction(dat => {
                string haystack = dat.num_args[0].asString();
                string needle = dat.num_args[1].asString();
                Var replacement = dat.num_args[2];
                Regex matcher = new Regex(needle);
                
                if(replacement is VarFunction){
                    StringBuilder result = new StringBuilder();
                    MatchCollection M = matcher.Matches(haystack);
                    int lastPoint = 0;
                    for(int i = 0; i < M.Count; i++){
                        Match m = M[i];
                        result.Append(haystack.Substring(lastPoint, m.Index - lastPoint));
                        lastPoint = m.Index + m.Length;
                        CallData cd = new CallData();
                        cd.num_args = new Dictionary<double, Var>();
                        cd.str_args = new Dictionary<string, Var>();
                        cd.var_args = new Dictionary<Var,    Var>();
                        if(m.Groups.Count > 1)
                            for(int c = 1; c < m.Groups.Count; c++){
                                string groupText = Regex.Match(needle, Regex.Replace(groupFinder, "3", ""+c)).Groups[1].Value;
                                if(groupText.Length == 0)
                                    cd.str_args[m.Groups[c].Name] = cd.num_args[c-1] = m.Groups[c].Index;
                                else
                                    cd.str_args[m.Groups[c].Name] = cd.num_args[c-1] = m.Groups[c].Value;
                            }
                        else
                            cd.num_args[0] = m.Groups[0].Value;
                        result.Append(replacement.Call(cd).asString().data);
                    }
                    result.Append(haystack.Substring(lastPoint));
                    return result.ToString();
                }else{
                    return matcher.Replace(haystack, replacement.asString());
                }
            });
            str["upper"] = new VarFunction(dat => dat.num_args[0].asString().data.ToUpper());
            str["lower"] = new VarFunction(dat => dat.num_args[0].asString().data.ToLower());
            str["reverse"] = new VarFunction(dat => {
                char[] arr = dat.num_args[0].asString().data.ToCharArray();
                Array.Reverse(arr);
                return new String(arr);
            });
            return str;
        }
    }
}