using System;

namespace Funky
{
    class Executer
    {
        static void Main(string[] args)
        {
            Meta.GetMeta();
            TProgram prog = TProgram.Claim(new StringClaimer(@"print(3 * 5 + 3 * 2)"));
            prog.Parse();
        }
    }
}
