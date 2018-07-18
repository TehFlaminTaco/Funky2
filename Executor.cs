using System;

namespace Funky
{
    class Executer
    {
        static void Main(string[] args)
        {
            Meta.GetMeta();
            TProgram prog = TProgram.Claim(new StringClaimer(@"{1; 2;}"));
            prog.Parse();
        }
    }
}
