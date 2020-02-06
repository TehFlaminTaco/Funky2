using Funky;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace Funky.Libs{
    public static class LibEvent{
        public static VarList Generate(){
            VarList evnt = new VarList();

            evnt["hook"] = new VarFunction(dat => dat.num_args[0].asEvent().Hook(dat.num_args[1]));
            evnt["unhook"] = new VarFunction(dat => dat.num_args[0].asEvent().Unhook(dat.num_args[1]));
            evnt["call"] = new VarFunction(dat => dat.num_args[0].Call(dat));
            evnt["new"] = new VarFunction(dat => new VarEvent(dat.num_args[0].asString()));

            return evnt;
        }
    }
}