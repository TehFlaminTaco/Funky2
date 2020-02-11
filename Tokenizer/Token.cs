
using System.Collections.Generic;
using System.Text;

namespace Funky.Tokens{
    abstract class Token{

        public string raw = "";
        public int offset = -1;
        public StringClaimer ownerClaimer;

        public static Token Claim(StringClaimer claimer){
            return null;
        }

        public void SetDebugInfo(StringClaimer claimer, int startPos){
            offset = startPos;
            raw = claimer.getTextFrom(startPos);
            ownerClaimer = claimer;
        }

        public void ShowError(System.Exception e){
            if(offset == -1){ // If we don't have debug info, pass it up.
                throw e;
            }
            //System.Console.Error.WriteLine($"Error at point {ownerClaimer.getLine(offset)}:{ownerClaimer.getChar(offset)} \"{raw}\"\n{e.Message}");
            // This should theoretically climb UP the call stack with the error untill something (Or nothing) catches it.
            throw new System.Exception($"at position {ownerClaimer.getLine(offset)}:{ownerClaimer.getChar(offset)} \"{raw.Split('\n')[0] + (raw.IndexOf("\n")>-1?"...":"")}\"\n{e.Message}");
        }
    }
}