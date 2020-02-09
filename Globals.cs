using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Funky.Libs;

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

                globals["math"] = LibMath.Generate();
                globals["string"] = LibString.Generate();
                globals["list"] = LibList.Generate();
                globals["io"] = LibIO.Generate();
                globals["os"] = LibOS.Generate();
                globals["event"] = LibEvent.Generate();
                globals["draw"] = LibDraw.Generate();

                globals["_G"] = globals;
            }
            return globals;
        }
    } 
}