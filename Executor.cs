using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Funky.Tokens;
using System;
using System.Collections.Generic;

namespace Funky
{
    public static class Executor
    {
        /*static void OldMain(string[] args)
        {
            var file = args.FirstOrDefault();
            var mainFile = new FunkyFile("main.fnk", "funky");
            var code = file is null
                ? (mainFile.Exists()?mainFile.ReadAllText():@"print('Please specify a file.')")
                : new FunkyFile(file, "funky", ".fnk").ReadAllText();
            var fileName = file is null ? "main.fnk" : new FunkyFile(file, "funky", ".fnk").shortName;
            var time = DateTime.Now;//.Subtract(programStart).TotalMilliseconds;
            Meta.GetMeta();
            try{
                VarList argList = new VarList();
                for(var i=0; i < args.Length; i++)
                    argList.double_vars[i] = args[i];
                ExecuteProgram(code, fileName, argList);
            }catch(System.Exception e){
                System.Console.Error.WriteLine($"ERROR:\n{e.Message}");
            }
        }*/

        static void Main(string[] args){
            var fileTarget = args.FirstOrDefault();
            FunkyFile file;
            if(fileTarget is null){
                file = new FunkyFile("main", "funky", ".fnk", ".sfnk", ".cfnk");
            }else{
                file = new FunkyFile(fileTarget, "funky", ".fnk", ".sfnk", ".cfnk");
            }
            if(!file.Exists()){
                Console.WriteLine("Please specify a valid file.");
                return;
            }
            VarList argList = new VarList();
            argList.double_vars[0] = file.realPath;
            for(int i=1; i < args.Length; i++){
                argList.double_vars[i] = args[i];
            }
            ExecuteProgram(argList);
        }

        public static bool DYNAMIC_RECOMPILE = false;
        public static Var ExecuteProgram(VarList args, Scope? s = null){
            bool wasDynamic = DYNAMIC_RECOMPILE;
            Var last = Var.nil;
            try{
                string fileTarget = args.double_vars.ContainsKey(0)?args.double_vars[0].asString().data:null;
                FunkyFile file;
                file = new FunkyFile(fileTarget, "funky", ".fnk", ".sfnk", ".cfnk");
                if(!file.Exists())return Var.nil;
                var ext = file.Extension;
                Meta.GetMeta();
                VarList argList = new VarList();
                for(int i=1; args.double_vars.ContainsKey(i); i++){
                    argList.double_vars[i-1] = args.double_vars[i];
                }
                Scope scope;
                if(s == null){
                    VarList scopeList = new VarList();
                    scopeList.parent = Globals.get();
                    scopeList.meta = new VarList();
                    if(argList!=null)
                        scopeList.string_vars["arg"] = argList;
                    scope = new Scope(scopeList);
                }else{
                    Scope oldScope = (Scope)s;
                    scope = new Scope();
                    scope.variables = new VarList();
                    scope.variables.parent = oldScope.variables;
                    scope.variables.meta = new VarList();
                    scope.escape = oldScope.escape;
                }
                if(ext == ".sfnk"){
                    // Read and compile
                    List<TExpression> expressions;
                    var targetFile = Path.ChangeExtension(file.realPath, ".cfnk");
                    var sourceEditTime = File.GetLastWriteTime(file.realPath);
                    if(!File.Exists(targetFile) || File.GetLastWriteTime(targetFile)<sourceEditTime){
                        var f = System.IO.File.Open(targetFile, FileMode.Create);
                        var writer = new BinaryWriter(f);
                        IBinaryReadWritable.WriteHeader(writer);
                        expressions = IBinaryReadWritable.WriteProgram(file.ReadAllText(), writer, file.shortName);
                        f.Close();
                        File.SetLastWriteTime(targetFile, sourceEditTime);
                    }else{
                        var f = System.IO.File.Open(targetFile, FileMode.Open);
                        var reader = new BinaryReader(f);
                        if(!IBinaryReadWritable.ReadHeader(reader)){
                            f.Close();
                            f = System.IO.File.Open(targetFile, FileMode.Create);
                            var writer = new BinaryWriter(f);
                            IBinaryReadWritable.WriteHeader(writer);
                            expressions = IBinaryReadWritable.WriteProgram(file.ReadAllText(), writer, file.shortName);
                            f.Close();
                            File.SetLastWriteTime(targetFile, sourceEditTime);
                        }
                        expressions = IBinaryReadWritable.ReadProgram(reader);
                        f.Close();
                    }
                    foreach(var expression in expressions){
                        last=expression.Parse(scope);
                    }
                }else if(ext == ".cfnk"){
                    var f = System.IO.File.Open(file.realPath, FileMode.Open);
                    var reader = new BinaryReader(f);
                    IBinaryReadWritable.ReadHeader(reader);
                    var expressions = IBinaryReadWritable.ReadProgram(reader);
                    f.Close();
                    foreach(var expression in expressions){
                        last=expression.Parse(scope);
                    }
                }else{
                    TExpression e;
                    StringClaimer claimer = new StringClaimer(file.ReadAllText(), file.shortName);
                    while((e=TExpression.Claim(claimer))!=null){
                        //Console.WriteLine($"Took {Math.Round(DateTime.Now.Subtract(lastClaimTime).TotalSeconds, 3)}s to Compile");
                        claimer.Claim(SEMI_COLON);
                        last = e.Parse(scope);
                    }
                    if(claimer.bestReach < claimer.to_claim.Length){
                        string errorString = CLEAN_ERROR.Replace(claimer.to_claim.Substring(claimer.bestReach), "...");
                        throw new FunkyException($"Unexpected symbol at {claimer.getLine(claimer.bestReach)}:{claimer.getChar(claimer.bestReach)} \"{errorString}\"");
                    }
                }
            }finally{
                DYNAMIC_RECOMPILE = wasDynamic;
            }
            return last;
        }

        public static Regex CLEAN_ERROR = new Regex(@"\n(\n|\r|.)*");
        public static Regex SEMI_COLON = new Regex(@"^;");
        /*public static Var ExecuteProgram(string code, string fileName, VarList cmdArgs = null){
            TExpression e;
            Var last = Var.nil;
            VarList scopeList = new VarList();
            scopeList.parent = Globals.get();
            scopeList.meta = new VarList();
            if(cmdArgs!=null)
                scopeList.string_vars["arg"] = cmdArgs;
            Scope scope = new Scope(scopeList);
            StringClaimer claimer = new StringClaimer(code, fileName);
            var lastClaimTime = DateTime.Now;
            while((e=TExpression.Claim(claimer))!=null){
                //Console.WriteLine($"Took {Math.Round(DateTime.Now.Subtract(lastClaimTime).TotalSeconds, 3)}s to Compile");
                claimer.Claim(SEMI_COLON);
                lastClaimTime = DateTime.Now;
                last = e.Parse(scope);
                //Console.WriteLine($"Took {Math.Round(DateTime.Now.Subtract(lastClaimTime).TotalSeconds, 3)}s to Parse");
                lastClaimTime = DateTime.Now;
            }
            if(claimer.bestReach < claimer.to_claim.Length){
                string errorString = CLEAN_ERROR.Replace(claimer.to_claim.Substring(claimer.bestReach), "...");
                throw new FunkyException($"Unexpected symbol at {claimer.getLine(claimer.bestReach)}:{claimer.getChar(claimer.bestReach)} \"{errorString}\"");
            }
            return last;
        }*/
    }
}
