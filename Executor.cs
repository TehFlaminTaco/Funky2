using System.Linq;
using System.IO;
namespace Funky
{
    static class Executer
    {
        static void Main(string[] args)
        {
            var file = args.FirstOrDefault();
            var mainFile = new FunkyFile("main.fnk", "funky");
            var code = file is null
                ? (mainFile.Exists()?mainFile.ReadAllText():@"print('Please specify a file.')")
                : new FunkyFile(file, "funky", ".fnk").ReadAllText();
            var fileName = file is null ? "main.fnk" : new FunkyFile(file, "funky", ".fnk").shortName;
            Meta.GetMeta();
            try{
                TProgram prog = TProgram.Claim(new StringClaimer(code, fileName));
                prog.Parse();
            }catch(System.Exception e){
                System.Console.Error.WriteLine($"ERROR:\n{e.Message}");
            }
        }
    }
}
