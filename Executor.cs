using System;

namespace Funky
{
    class Executer
    {
        static void Main(string[] args)
        {
            Meta.GetMeta();
            TProgram prog = TProgram.Claim(new StringClaimer(@"
            for i=0 i<10 i+=1
                print(i)
        "));
            prog.Parse();
        }
    }
}
