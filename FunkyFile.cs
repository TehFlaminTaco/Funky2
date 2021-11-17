using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Funky{
    public abstract class FunkyFS{
        public string realPath = null;

        public string shortName { get { return Path.GetFileName(realPath); } set {} }

        public static string[] searchLocations = {"./", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)};
        
        public FunkyFS(){}

        public FunkyFS(string name){
            foreach(String location in searchLocations){
                string cur = Path.Combine(location, name);
                if(CheckFile(cur)){
                    realPath = cur;
                    break;
                }
            }
        }

        public bool Exists(){
            return realPath != null;
        }

        public abstract bool CheckFile(string path);
    }

    public class FunkyFile : FunkyFS{
        public string Extension => Path.GetExtension(realPath);

        public FunkyFile(string name) : base(name){}

        public FunkyFile(string name, string specialFolder, params string[] guessExtensions) : base(){
            string[] withSpecial = new string[]{"", specialFolder};
            IEnumerable<string> gE = guessExtensions.Prepend("");
            foreach(String location in searchLocations){
                foreach(String special in withSpecial){
                    foreach(String ext in gE){
                        string cur = Path.Combine(location, special, name) + ext;
                        if(CheckFile(cur)){
                            realPath = cur;
                            return;
                        }
                    }
                }
            }
        }

        public override bool CheckFile(string path){
            return File.Exists(path);
        }

        public string ReadAllText(){
            if(realPath == null)
                throw new FileNotFoundException();
            return File.ReadAllText(realPath);
        }
    }

    public class FunkyFolder : FunkyFS {
        public FunkyFolder(string name) : base(name){}
        public override bool CheckFile(string path){
            return Directory.Exists(path);
        }
    }
}