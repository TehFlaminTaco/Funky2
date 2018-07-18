using System;

namespace Funky
{
    class Executer
    {
        static void Main(string[] args)
        {
            Meta.GetMeta();
            TProgram prog = TProgram.Claim(new StringClaimer(@"
            print(""Hello, World!"")
        "));
            prog.Parse();
        }
    }
}
