using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Funky.Libs;
using Funky.Tokens;

namespace Funky{
class Globals{
        static VarList globals = null;
        public static VarList get(){
            if(globals == null){
                globals = new VarList();
                globals.meta = new VarList();
                globals["print"] = new VarFunction(dat => {
                    StringBuilder sb = new StringBuilder();
                    string chunker = "";
                    for(int i=0; dat._num_args.ContainsKey(i); i++){
                        sb.Append(chunker);
                        sb.Append(dat._num_args[i].asString());
                        chunker = "\t";
                    }
                    string outStr = sb.ToString();
                    Console.WriteLine(outStr);
                    return outStr;
                });
                globals["write"] = new VarFunction(dat => {
                    StringBuilder sb = new StringBuilder();
                    for(int i=0; dat._num_args.ContainsKey(i); i++){
                        sb.Append(dat._num_args[i].asString());
                    }
                    string outStr = sb.ToString();
                    Console.Write(outStr);
                    return outStr;
                });
                globals["error"] = new VarFunction(dat => {
                    throw new FunkyException(dat.Get(0).Or("message").Otherwise("").GetString());
                });

                globals["setscope"] = new VarFunction(dat => {
                    VarFunction fnc = dat.Get(0).Or("function").As(ArgType.Function).Required().GetFunction();
                    VarList scope = dat.Get(1).Or("scope").As(ArgType.List).Required().GetList();
                    if(fnc.scope.escape != null){
                        fnc.scope.variables = scope.asList();
                    }
                    return fnc;
                });
                globals["getscope"] = new VarFunction(dat => {
                    VarFunction fnc = dat.Get(0).Or("function").As(ArgType.Function).Required().GetFunction();
                    if(fnc.scope.escape != null){
                        return fnc.scope.variables;
                    }
                    return Var.nil;
                });

                globals["require"] = new VarFunction(dat => {
                    string fileName = dat.Get(0).Or("file").As(ArgType.String).Required().GetString();
                    var file = new FunkyFile(fileName, "funky", ".fnk");
                    if(!file.Exists()){
                        throw new FunkyException("File not found.");
                    }else{
                        return Executor.ExecuteProgram(file.ReadAllText(), file.shortName);
                    }
                });

                globals["tonumber"] = new VarFunction(dat => dat.Get(0).Required().Get().asNumber());

                globals["math"] = LibMath.Generate();
                globals["string"] = LibString.Generate();
                globals["list"] = LibList.Generate();
                globals["io"] = LibIO.Generate();
                globals["os"] = LibOS.Generate();
                globals["event"] = LibEvent.Generate();
                if (System.Environment.OSVersion.VersionString.IndexOf("Windows")>=0)
                    globals["draw"] = LibDraw.Generate(); // Draw is only supported on windows for now.
                globals["sound"] = LibSound.Generate();

                globals["_G"] = globals;
            }
            return globals;
        }
    } 
}