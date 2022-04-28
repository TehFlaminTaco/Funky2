using Funky;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Funky.Libs{
    public static class LibIO{
        private static Type FromString(string s){
            return s.ToLower() switch {
                "str" or "string" => typeof(string),
                "int64" or "long" => typeof(long),
                "uint64" or "ulong" => typeof(ulong),
                "int" or "int32" => typeof(int),
                "uint" or "uint32" => typeof(uint),
                "short" => typeof(short),
                "ushort" => typeof(ushort),
                "char" or "sbyte" => typeof(sbyte),
                "uchar" or "byte" => typeof(byte),
                "float" => typeof(float),
                "double" => typeof(double),
                "ptr" or "intptr" => typeof(IntPtr),
                _ => typeof(int)
            };
        }

        private static object FromVar(string typstring, Var v){
            return typstring.ToLower() switch {
                "str" or "string" => v.asString().data,
                "int64" or "long" => (long)v.asNumber().value,
                "uint64" or "ulong" => (ulong)v.asNumber().value,
                "int" or "int32" => (int)v.asNumber().value,
                "uint" or "uint32" => (uint)v.asNumber().value,
                "short" => (short)v.asNumber().value,
                "ushort" => (ushort)v.asNumber().value,
                "char" or "sbyte" => (sbyte)v.asNumber().value,
                "uchar" or "byte" => (byte)v.asNumber().value,
                "float" => (float)v.asNumber().value,
                "double" => (double)v.asNumber().value,
                "ptr" or "intptr" => (IntPtr)v.asNumber().value,
                _ => null
            };
        }

        private static Var ToVar(string typstring, object o){
            return typstring.ToLower() switch {
                "str" or "string" => new VarString((string)o),
                "int64" or "long" => new VarNumber((long)o),
                "uint64" or "ulong" => new VarNumber((ulong)o),
                "int" or "int32" => new VarNumber((int)o),
                "uint" or "uint32" => new VarNumber((uint)o),
                "short" => new VarNumber((short)o),
                "ushort" => new VarNumber((ushort)o),
                "char" or "sbyte" => new VarNumber((sbyte)o),
                "uchar" or "byte" => new VarNumber((byte)o),
                "float" => new VarNumber((float)o),
                "double" => new VarNumber((double)o),
                "ptr" or "intptr" => new VarNumber((int)(IntPtr)o),
                _ => null
            };
        }
        
        public static VarList Generate(){
            VarList io = new VarList();

            io["entries"] = new VarFunction(dat => {
                VarString path = dat.Get(0).Or("folder").Otherwise("").GetString();
                FunkyFolder folder = new FunkyFolder(path);
                if(folder.Exists()){
                    VarList results = new VarList();
                    int i = 0;
                    foreach(string name in Directory.EnumerateFileSystemEntries(folder.realPath)){
                        results[i++] = name;
                    }
                    return results;
                }
                return Var.nil;
            });
            io["files"] = new VarFunction(dat => {
                VarString path = dat.Get(0).Or("folder").Otherwise("").GetString();
                FunkyFolder folder = new FunkyFolder(path);
                if(folder.Exists()){
                    VarList results = new VarList();
                    int i = 0;
                    foreach(string name in Directory.EnumerateFiles(folder.realPath)){
                        results[i++] = name;
                    }
                    return results;
                }
                return Var.nil;
            });
            io["folders"] = new VarFunction(dat => {
                VarString path = dat.Get(0).Or("folder").Otherwise("").GetString();
                FunkyFolder folder = new FunkyFolder(path);
                if(folder.Exists()){
                    VarList results = new VarList();
                    int i = 0;
                    foreach(string name in Directory.EnumerateDirectories(folder.realPath)){
                        results[i++] = name;
                    }
                    return results;
                }
                return Var.nil;
            });
            io["exists"] = new VarFunction(dat => {
                return (
                    new FunkyFile(dat.Get(0).Or("file").Required().GetString()).Exists() ||
                    new FunkyFolder(dat.Get(0).Or("file").Required().GetString()).Exists()) ? 1 : 0;
            });
            io["isfolder"] = io["isdirectory"] = new VarFunction(dat => {
                return new FunkyFolder(dat.Get(0).Or("file").Required().GetString()).Exists() ? 1 : 0;
            });
            io["open"] = new VarFunction(dat => {
                FunkyFile f = new FunkyFile(dat.Get(0).Or("file").Required().GetString());
                string flags = dat.Get(1).Or("flags").Otherwise("r").GetString();
                VarList file = new VarList();

                FileStream fs = null;
                FileAccess rw = FileAccess.Read;
                if(flags.IndexOf("r")>=0 && flags.IndexOf("w")>=0)
                    rw = FileAccess.ReadWrite;
                else if(flags.IndexOf("w")>=0)
                    rw = FileAccess.Write;
                if(f.Exists()){
                    fs = new FileStream(f.realPath, FileMode.OpenOrCreate, rw);
                }else{
                    fs = new FileStream(dat.Get(0).Or("file").Required().GetString(), FileMode.OpenOrCreate, rw);
                }

                file["exists"] = new VarFunction(dat => f.Exists()?1:0);
                file["readall"] = new VarFunction(dat => f.ReadAllText());
                file["readline"] = new VarFunction(dat => {
                    List<byte> line = new List<byte>();
                    int b;
                    while(fs.Position<fs.Length && (b=fs.ReadByte())!=10){
                        line.Add((byte)b);
                    }
                    return Encoding.UTF8.GetString(line.ToArray());
                });
                file["write"] = new VarFunction(dat => {
                    string text = dat.Get(0).Or("text").Required().GetString();
                    fs.Write(Encoding.UTF8.GetBytes(text));
                    return file;
                });
                file["close"] = new VarFunction(dat => {
                    fs.Close();
                    return Var.nil;
                });


                return file;
            });
            io["loaddll"] = new VarFunction(dat => {
                string dll = dat.Get(0).Or("dll").Required().GetString();
                string func = dat.Get(1).Or("func").Required().GetString();
                string returnType = dat.Get(2).Or("type").Required().GetString();
                Type retType = FromString(returnType);
                List<Type> args = new();
                for(int i=3; dat._num_args.ContainsKey(i); i++){
                    string t = dat._num_args[i].asString();
                    args.Add(FromString(t));
                }
                var lib = FunkyHelpers.LoadLibrary(dll.ToString());
                if(lib==IntPtr.Zero){
                    switch(Marshal.GetLastWin32Error()){
                        case(0x7E):{
                            throw new Exception("DLL not found");
                        }
                        default:{
                            throw new Exception($"Unknown error ({Marshal.GetLastWin32Error():x8})");
                        }
                    }
                }
                var method = FunkyHelpers.GetProcAddress(lib, func.ToString());
                if(method==IntPtr.Zero){
                    switch(Marshal.GetLastWin32Error()){
                        case(0x7F):{
                            throw new Exception("Proc not found");
                        }
                        default:{
                            throw new Exception($"Unknown error ({Marshal.GetLastWin32Error():x8})");
                        }
                    }
                }
                
                var methodType = DelegateCreator.NewDelegateType(retType, args.ToArray());
                var delg = Marshal.GetDelegateForFunctionPointer(method, methodType);
                return new VarFunction(cd=>{
                    object[] convertedArgs = new object[args.Count];
                    for(int i=0; i < convertedArgs.Length; i++){
                        if(cd._num_args.ContainsKey(i)){
                            convertedArgs[i] = FromVar(dat._num_args[3+i].asString(), cd._num_args[i]);
                        }
                    }
                    return ToVar(returnType,delg.DynamicInvoke(convertedArgs));
                });
            });
            io["stdin"] = new VarList();
            return io;
        }
    }
}