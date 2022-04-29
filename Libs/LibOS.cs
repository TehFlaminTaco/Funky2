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
            os["sleep"] = new VarFunction(dat => {
                double ms = dat.Get(0).GetNumber();
                System.Threading.Thread.Sleep((int)ms);
                return Var.nil;
            });
            os["createthread"] = new VarFunction(dat => {
                var fnc = dat.Get(0).GetFunction();
                var thrd = new System.Threading.Thread(()=>fnc.action(new CallData()));
                thrd.IsBackground = true;
                thrd.Start();
                var thrdList = new VarList();
                thrdList["handle"] = thrd.ManagedThreadId;
                return thrdList;
            });
            return os;
        }
    }
}