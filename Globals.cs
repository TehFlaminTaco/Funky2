using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Funky.Libs;
using Funky.Tokens;
using System.Threading;
using System.Linq;

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
                    var file = new FunkyFile(fileName, "funky", ".fnk", ".sfnk", ".cfnk");
                    if(!file.Exists()){
                        throw new FunkyException("File not found.");
                    }else{
                        VarList args = new VarList();
                        args.double_vars[0] = file.realPath;
                        for(int i=1; dat._num_args.ContainsKey(i); i++){
                            args.double_vars[i] = dat._num_args[i];
                        }
                        foreach(var kv in dat._str_args){
                            args.string_vars[kv.Key] = kv.Value;
                        }
                        foreach(var kv in dat._var_args){
                            args.other_vars[kv.Key] = kv.Value;
                        }
                        return Executor.ExecuteProgram(args);
                    }
                });

                globals["tonumber"] = new VarFunction(dat => dat.Get(0).Required().Get().asNumber());

                globals["type"] = new VarFunction(dat => dat.Get(0).Get().type);

                var tossError = new VarFunction(dat => {throw new FunkyException(dat.Get(0).Otherwise("Timeout Exceeded.").GetString());});

                globals["timeout"] = new VarFunction(dat => {
                    var time = dat.Get(0).Or("time").Required().GetNumber().value;
                    var action = dat.Get(1).Or("action").Required().GetFunction();
                    var onError = dat.Get(2).Or("catch").Otherwise(tossError).GetFunction();
                    var currentTime = DateTime.Now;
                    bool running = true;
                    new Thread(() => {
                        while(DateTime.Now.Subtract(currentTime).TotalMilliseconds <  time && running){
                            Thread.Sleep(10);
                        }
                        if(running){
                            action.scope.escape.Push(new Escaper(Escape.TIMEOUT, Var.nil));
                        }
                    }).Start();
                    try {
                        var outp = action.Call(new CallData());
                        running = false;
                        if(action.scope.escape.Any(c=>c.method== Escape.TIMEOUT)){ // Did Timeout.
                            action.scope.escape.Clear();
                            return onError.Call(new CallData());
                        }
                        return outp;
                    }catch(Exception e){
                        running = false;
                        throw e;
                    }
                });

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