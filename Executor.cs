using System.Linq;
using System.IO;
namespace Funky
{
    static class Executer
    {
        static void Main(string[] args)
        {
            var file = args.FirstOrDefault();
            
            var code = file is null
                ? (File.Exists("main.fnk")?File.ReadAllText("main.fnk"):@"print('Please specify a file.')")
                : File.ReadAllText(file);
            Meta.GetMeta();
            try{
                TProgram prog = TProgram.Claim(new StringClaimer(code));
                prog.Parse();
            }catch(System.Exception e){
                System.Console.Error.WriteLine($"ERROR:\n{e.Message}");
            }
        }
    }
}
