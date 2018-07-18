using System;

namespace Funky
{
    class Executer
    {
        static void Main(string[] args)
        {
            Meta.GetMeta();
            TProgram prog = TProgram.claim(new StringClaimer(@"print(5 + 3)"));
            prog.Parse();
        }
    }
}
