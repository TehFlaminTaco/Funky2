using Funky;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace Funky.Libs{
    public static class LibOS{
        public static DateTime programStart = DateTime.Now;
        public static VarList Generate(){
            VarList os = new VarList();

            os["clock"] = os["millis"] = new VarFunction(dat => DateTime.Now.Subtract(programStart).TotalMilliseconds);

            return os;
        }
    }
}