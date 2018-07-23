using System.Text;
using System.Collections.Generic;
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

           }
           return globals;
       }
   } 
}