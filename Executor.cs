using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Funky.Tokens;
using System;

namespace Funky
{
    public static class Executor
    {
        static void Main(string[] args)
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
        }

        public static Regex CLEAN_ERROR = new Regex(@"\n(\n|\r|.)*");
        public static Regex SEMI_COLON = new Regex(@"^;");
        public static Var ExecuteProgram(string code, string fileName, VarList cmdArgs = null){
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
                Console.WriteLine($"Took {Math.Round(DateTime.Now.Subtract(lastClaimTime).TotalSeconds, 3)}s to Compile");
                claimer.Claim(SEMI_COLON);
                lastClaimTime = DateTime.Now;
                last = e.Parse(scope);
                Console.WriteLine($"Took {Math.Round(DateTime.Now.Subtract(lastClaimTime).TotalSeconds, 3)}s to Parse");
                lastClaimTime = DateTime.Now;
            }
            if(claimer.bestReach < claimer.to_claim.Length){
                string errorString = CLEAN_ERROR.Replace(claimer.to_claim.Substring(claimer.bestReach), "...");
                throw new FunkyException($"Unexpected symbol at {claimer.getLine(claimer.bestReach)}:{claimer.getChar(claimer.bestReach)} \"{errorString}\"");
            }
            return last;
        }
    }
}
