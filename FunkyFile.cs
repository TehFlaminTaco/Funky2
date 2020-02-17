using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Funky{
    public class FunkyFile{
        public string realPath = null;

        public string shortName { get { return Path.GetFileName(realPath); } set {} }

        public string[] searchLocations = {"./", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)};

        public FunkyFile(string name){
            foreach(String location in searchLocations){
                string cur = Path.Combine(location, name);
                if(File.Exists(cur)){
                    realPath = cur;
                    break;
                }
            }
        }

        public FunkyFile(string name, string specialFolder, params string[] guessExtensions){
            string[] withSpecial = new string[]{"", specialFolder};
            IEnumerable<string> gE = guessExtensions.Prepend("");
            foreach(String location in searchLocations){
                foreach(String special in withSpecial){
                    foreach(String ext in gE){
                        string cur = Path.Combine(location, special, name) + ext;
                        if(File.Exists(cur)){
                            realPath = cur;
                            return;
                        }
                    }
                }
            }
        }

        public string ReadAllText(){
            if(realPath == null)
                throw new FileNotFoundException();
            return File.ReadAllText(realPath);
        }

        public bool Exists(){
            return realPath != null;
        }
    }
}