using Funky;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Funky.Libs{
    public static class LibIO{
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
            io["stdin"] = new VarList();
            return io;
        }
    }
}