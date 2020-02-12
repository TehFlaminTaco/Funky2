using Funky;
using System;

namespace Funky.Libs{
    public static class LibMath{
        public static VarList Generate(){
            VarList math = new VarList();

            math["sin"] = new VarFunction(dat => Math.Sin(dat.Get(0).Required().GetNumber()));
            math["cos"] = new VarFunction(dat => Math.Cos(dat.Get(0).Required().GetNumber()));
            math["tan"] = new VarFunction(dat => Math.Tan(dat.Get(0).Required().GetNumber()));
            math["asin"] = new VarFunction(dat => Math.Asin(dat.Get(0).Required().GetNumber()));
            math["acos"] = new VarFunction(dat => Math.Acos(dat.Get(0).Required().GetNumber()));
            math["atan"] = new VarFunction(dat => {
                if(dat._num_args.ContainsKey(1))
                    return Math.Atan2(dat.Get(0).Required().GetNumber(), dat.Get(1).Required().GetNumber());
                else
                    return Math.Atan(dat.Get(0).Required().GetNumber());
            });
            math["floor"] = new VarFunction(dat => {
                if(dat._num_args.ContainsKey(1)){
                    double e = Math.Pow(10, dat.Get(1).Required().GetNumber());
                    return Math.Floor(dat.Get(0).Required().GetNumber()*e)/e;
                }else
                    return Math.Floor(dat.Get(0).Required().GetNumber());
            });
            math["ceil"] = new VarFunction(dat => {
                if(dat._num_args.ContainsKey(1)){
                    double e = Math.Pow(10, dat.Get(1).Required().GetNumber());
                    return Math.Ceiling(dat.Get(0).Required().GetNumber()*e)/e;
                }else
                    return Math.Ceiling(dat.Get(0).Required().GetNumber());
            });
            math["round"] = new VarFunction(dat => {
                if(dat._num_args.ContainsKey(1)){
                    double e = Math.Pow(10, dat.Get(1).Required().GetNumber());
                    return Math.Floor(0.5d+dat.Get(0).Required().GetNumber()*e)/e;
                }else
                    return Math.Floor(0.5d+dat.Get(0).Required().GetNumber());
            });
            math["min"] = new VarFunction(dat => Math.Min(dat.Get(0).Required().GetNumber(), dat.Get(1).Required().GetNumber()));
            math["max"] = new VarFunction(dat => Math.Max(dat.Get(0).Required().GetNumber(), dat.Get(1).Required().GetNumber()));
            math["clamp"] = new VarFunction(dat => Math.Clamp(dat.Get(0).Required().GetNumber(), dat.Get(1).Required().GetNumber(), dat.Get(2).Required().GetNumber()));
            Random rng = new Random();
            math["random"] = new VarFunction(dat => {
                if(dat._num_args.ContainsKey(1)){
                    return rng.Next((int)dat.Get(0).Required().GetNumber(), (int)dat.Get(1).Required().GetNumber());
                }else if(dat._num_args.ContainsKey(0)){
                    return rng.Next((int)dat.Get(0).Required().GetNumber());
                }else{
                    return rng.NextDouble();
                }
            });
            math["abs"] = new VarFunction(dat => Math.Abs(dat.Get(0).Required().GetNumber()));
            math["deg"] = new VarFunction(dat => dat.Get(0).Required().GetNumber() * (180.0d / Math.PI));
            math["rad"] = new VarFunction(dat => Math.PI * dat.Get(0).Required().GetNumber() / 180.0d);
            math["sqrt"] = new VarFunction(dat => Math.Sqrt(dat.Get(0).Required().GetNumber()));
            math["pi"] = Math.PI;
            math["dot"] = new VarFunction(dat => {
                float x1 = (float)dat.Get(0).Or("x1").Otherwise(0).GetNumber();
                float y1 = (float)dat.Get(1).Or("y1").Otherwise(0).GetNumber();
                float x2 = (float)dat.Get(2).Or("x2").Otherwise(0).GetNumber();
                float y2 = (float)dat.Get(3).Or("y2").Otherwise(0).GetNumber();
                return System.Numerics.Vector2.Dot(new System.Numerics.Vector2(x1, y1), new System.Numerics.Vector2(x2, y2));
            });
            return math;
        }
    }
}