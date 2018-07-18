using System;

namespace Funky
{
    class Executer
    {
        static void Main(string[] args)
        {
            VarList l = Meta.meta;
            TProgram prog = TProgram.claim(new StringClaimer(@"x = 3; print(x)"));
            prog.Parse();
        }
    }
}
