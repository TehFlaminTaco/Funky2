using Funky;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace Funky.Libs{
    public static class LibEvent{
        public static VarList Generate(){
            VarList evnt = new VarList();

            evnt["hook"] = new VarFunction(dat => dat.Get(0).Required().GetEvent().Hook(dat.Get(1).Required().Get()));
            evnt["unhook"] = new VarFunction(dat => dat.Get(0).Required().GetEvent().Unhook(dat.Get(1).Required().Get()));
            evnt["call"] = new VarFunction(dat => dat.Get(0).Required().Get().Call(dat));
            evnt["new"] = new VarFunction(dat => new VarEvent(dat.Get(0).Required().GetString()));

            return evnt;
        }
    }
}