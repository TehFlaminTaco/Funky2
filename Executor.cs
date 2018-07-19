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
                ? @"
                    for i=0 i<10 i+=1
                        print(i)
                "
                : File.ReadAllText(file);
            Meta.GetMeta();
            TProgram prog = TProgram.Claim(new StringClaimer(code));
            prog.Parse();
        }
    }
}
