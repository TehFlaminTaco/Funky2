using Funky;
using System;

namespace Funky.Libs{
    public static class LibMath{
        public static VarList Generate(){
            VarList math = new VarList();

            math["sin"] = new VarFunction(dat => Math.Sin(dat.num_args[0].asNumber()));
            math["cos"] = new VarFunction(dat => Math.Cos(dat.num_args[0].asNumber()));
            math["tan"] = new VarFunction(dat => Math.Tan(dat.num_args[0].asNumber()));
            math["asin"] = new VarFunction(dat => Math.Asin(dat.num_args[0].asNumber()));
            math["acos"] = new VarFunction(dat => Math.Acos(dat.num_args[0].asNumber()));
            math["atan"] = new VarFunction(dat => Math.Atan(dat.num_args[0].asNumber()));
            math["floor"] = new VarFunction(dat => {
                if(dat.num_args.ContainsKey(1)){
                    double e = Math.Pow(10, dat.num_args[1].asNumber());
                    return Math.Floor(dat.num_args[0].asNumber()*e)/e;
                }else
                    return Math.Floor(dat.num_args[0].asNumber());
            });
            math["ceil"] = new VarFunction(dat => {
                if(dat.num_args.ContainsKey(1)){
                    double e = Math.Pow(10, dat.num_args[1].asNumber());
                    return Math.Ceiling(dat.num_args[0].asNumber()*e)/e;
                }else
                    return Math.Ceiling(dat.num_args[0].asNumber());
            });
            math["round"] = new VarFunction(dat => {
                if(dat.num_args.ContainsKey(1)){
                    double e = Math.Pow(10, dat.num_args[1].asNumber());
                    return Math.Floor(0.5d+dat.num_args[0].asNumber()*e)/e;
                }else
                    return Math.Floor(0.5d+dat.num_args[0].asNumber());
            });
            math["min"] = new VarFunction(dat => Math.Min(dat.num_args[0].asNumber(), dat.num_args[1].asNumber()));
            math["max"] = new VarFunction(dat => Math.Max(dat.num_args[0].asNumber(), dat.num_args[1].asNumber()));
            math["clamp"] = new VarFunction(dat => Math.Clamp(dat.num_args[0].asNumber(), dat.num_args[1].asNumber(), dat.num_args[2].asNumber()));
            Random rng = new Random();
            math["random"] = new VarFunction(dat => {
                if(dat.num_args.ContainsKey(1)){
                    return rng.Next((int)dat.num_args[0].asNumber(), (int)dat.num_args[1].asNumber());
                }else if(dat.num_args.ContainsKey(0)){
                    return rng.Next((int)dat.num_args[0].asNumber());
                }else{
                    return rng.NextDouble();
                }
            });
            math["abs"] = new VarFunction(dat => Math.Abs(dat.num_args[0].asNumber()));
            math["deg"] = new VarFunction(dat => dat.num_args[0].asNumber() * (180.0d / Math.PI));
            math["rad"] = new VarFunction(dat => Math.PI * dat.num_args[0].asNumber() / 180.0d);
            math["sqrt"] = new VarFunction(dat => Math.Sqrt(dat.num_args[0].asNumber()));
            math["pi"] = Math.PI;
            return math;
        }
    }
}