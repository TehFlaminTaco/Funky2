using System;

namespace Funky
{
    class Executer
    {
        static void Main(string[] args)
        {
            Meta.GetMeta();
            TProgram prog = TProgram.Claim(new StringClaimer(@"
            var x = 3;
            print(x)
            {
                print(x)
                var x = 6
                var y = 3
                print(x, y)
            }
            print(x, y)
        "));
            prog.Parse();
        }
    }
}
