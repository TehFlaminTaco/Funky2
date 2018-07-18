using System.Text;
using System;
namespace Funky{
   class Globals{
       static VarList globals;
       public static VarList get(){
           if(globals == null){
                globals = new VarList();

                globals["print"] = new VarFunction(delegate(CallData dat){
                    StringBuilder sb = new StringBuilder();
                    string chunker = "";
                    for(int i=0; dat.num_args.ContainsKey(i); i++){
                        sb.Append(chunker);
                        sb.Append(dat.num_args[i]?.asString() ?? "null");
                        chunker = "\t";
                    }
                    string outStr = sb.ToString();
                    Console.WriteLine(outStr);
                    return outStr;
                });

           }
           return globals;
       }
   } 
}