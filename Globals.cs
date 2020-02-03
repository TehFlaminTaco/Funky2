using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
namespace Funky{
class Globals{
        static VarList globals = null;
        public static VarList get(){
            if(globals == null){
                globals = new VarList();

                globals["print"] = new VarFunction(dat => {
                    StringBuilder sb = new StringBuilder();
                    string chunker = "";
                    for(int i=0; dat.num_args.ContainsKey(i); i++){
                        sb.Append(chunker);
                        sb.Append(dat.num_args[i].asString());
                        chunker = "\t";
                    }
                    string outStr = sb.ToString();
                    Console.WriteLine(outStr);
                    return outStr;
                });
                globals["write"] = new VarFunction(dat => {
                    StringBuilder sb = new StringBuilder();
                    for(int i=0; dat.num_args.ContainsKey(i); i++){
                        sb.Append(dat.num_args[i].asString());
                    }
                    string outStr = sb.ToString();
                    Console.Write(outStr);
                    return outStr;
                });
                globals["list"] = new VarFunction(dat => {
                    VarList l = new VarList();
                    l.double_vars = dat.num_args;
                    l.string_vars = dat.str_args;
                    l.other_vars = dat.var_args;
                    return l;
                });
                globals["setmeta"] = new VarFunction(dat => {
                    dat.num_args[0].meta = dat.num_args[1]?.asList();
                    return dat.num_args[0];
                });
                globals["setscope"] = new VarFunction(dat => {
                    VarFunction fnc = dat.num_args[0].asFunction();
                    if(fnc.scope.escape != null){
                        fnc.scope.variables = dat.num_args[1].asList();
                    }
                    return fnc;
                });
                globals["getscope"] = new VarFunction(dat => {
                    VarFunction fnc = dat.num_args[0].asFunction();
                    if(fnc.scope.escape != null){
                        return fnc.scope.variables;
                    }
                    return Var.nil;
                });

                globals["math"] = LibMath();
                globals["string"] = LibString();
                globals["list"] = LibList();
                globals["io"] = LibIO();

                globals["_G"] = globals;
            }
            return globals;
        }

        public static VarList LibMath(){
            VarList math = new VarList();

            math["sin"] = new VarFunction(dat => Math.Sin(dat.num_args[0].asNumber()));
            math["cos"] = new VarFunction(dat => Math.Cos(dat.num_args[0].asNumber()));
            math["tan"] = new VarFunction(dat => Math.Tan(dat.num_args[0].asNumber()));
            math["asin"] = new VarFunction(dat => Math.Asin(dat.num_args[0].asNumber()));
            math["acos"] = new VarFunction(dat => Math.Acos(dat.num_args[0].asNumber()));
            math["atan"] = new VarFunction(dat => Math.Atan(dat.num_args[0].asNumber()));
            math["floor"] = new VarFunction(dat => {
                if(dat.num_args.ContainsKey(1)){
                    double e = Math.Pow(10, dat.num_args[1].asNumber());
                    return Math.Floor(dat.num_args[0].asNumber()*e)/e;
                }else
                    return Math.Floor(dat.num_args[0].asNumber());
            });
            math["ceil"] = new VarFunction(dat => {
                if(dat.num_args.ContainsKey(1)){
                    double e = Math.Pow(10, dat.num_args[1].asNumber());
                    return Math.Ceiling(dat.num_args[0].asNumber()*e)/e;
                }else
                    return Math.Ceiling(dat.num_args[0].asNumber());
            });
            math["round"] = new VarFunction(dat => {
                if(dat.num_args.ContainsKey(1)){
                    double e = Math.Pow(10, dat.num_args[1].asNumber());
                    return Math.Floor(0.5d+dat.num_args[0].asNumber()*e)/e;
                }else
                    return Math.Floor(0.5d+dat.num_args[0].asNumber());
            });
            math["min"] = new VarFunction(dat => Math.Min(dat.num_args[0].asNumber(), dat.num_args[1].asNumber()));
            math["max"] = new VarFunction(dat => Math.Max(dat.num_args[0].asNumber(), dat.num_args[1].asNumber()));
            math["clamp"] = new VarFunction(dat => Math.Clamp(dat.num_args[0].asNumber(), dat.num_args[1].asNumber(), dat.num_args[2].asNumber()));
            Random rng = new Random();
            math["random"] = new VarFunction(dat => {
                if(dat.num_args.ContainsKey(1)){
                    return rng.Next((int)dat.num_args[0].asNumber(), (int)dat.num_args[1].asNumber());
                }else if(dat.num_args.ContainsKey(0)){
                    return rng.Next((int)dat.num_args[0].asNumber());
                }else{
                    return rng.NextDouble();
                }
            });
            math["abs"] = new VarFunction(dat => Math.Abs(dat.num_args[0].asNumber()));
            math["deg"] = new VarFunction(dat => dat.num_args[0].asNumber() * (180.0d / Math.PI));
            math["rad"] = new VarFunction(dat => Math.PI * dat.num_args[0].asNumber() / 180.0d);
            math["pi"] = new VarFunction(dat => Math.PI);
            return math;
        }

        public static VarList LibString(){
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

        public static VarList LibList(){
            VarList list = new VarList();

            return list;
        }

        public static VarList LibIO(){
            VarList io = new VarList();

            return io;
        }
    } 
}