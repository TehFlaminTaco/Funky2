
using System.ComponentModel;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Funky.Tokens{
    

    abstract class Token{
        public char TokenID => tokenTypes.Where(c=>c.Value==this.GetType()).First().Key;
        public static Dictionary<char, Type> tokenTypes = new Dictionary<char, Type>();
        public static char RegisterTokenType(char id, Type type){
            if(!typeof(Token).IsAssignableFrom(type)){
                throw new Exception("Unable to register non-token type");
            }
            if(tokenTypes.ContainsKey(id)){
                throw new Exception($"Token type of ID {id} already assigned to {tokenTypes[id]}");
            }
            tokenTypes.Add(id, type);
            return id;
        }

        public static void WriteToken(BinaryWriter writer, Token token){
            writer.Write(token.TokenID);
            token.TokenToBinary(writer);
        }
        
        public static void WriteToken(BinaryWriter writer, List<Token> token){
            writer.Write(BitConverter.GetBytes(token.Count));
            for(int i = 0; i < token.Count; i++){
                WriteToken(writer, token[i]);
            }
        }

        public static Token ReadToken(BinaryReader reader){
            int idOrEOF = reader.Read();
            if(idOrEOF < 0)return null;
            char id = (char)idOrEOF;
            if(id == 0)return null;
            if(!tokenTypes.ContainsKey(id)){
                throw new Exception($"Unknown token of index: {id}");
            }
            Type t=tokenTypes[id];
            Token tok = (Token)Activator.CreateInstance(t);
            return tok.BinaryToToken(reader);
        }

        public static List<Token> ReadTokenList(BinaryReader reader){
            int count = reader.ReadInt32();
            List<Token> tokens = new List<Token>();
            for(int i = 0; i < count; i++){
                tokens.Add(ReadToken(reader));
            }
            return tokens;
        }

        public string raw = "";
        public int offset = -1;
        public StringClaimer ownerClaimer;

        public static Token Claim(StringClaimer claimer){
            return null;
        }

        private static Regex endWhitespace = new Regex(@"\s*$");
        public void SetDebugInfo(StringClaimer claimer, int startPos){
            offset = startPos;
            raw = endWhitespace.Replace(claimer.getTextFrom(startPos), "");
            ownerClaimer = claimer;
        }

        public void ShowError(System.Exception e){
            if(offset == -1){ // If we don't have debug info, pass it up.
                throw e;
            }
            //System.Console.Error.WriteLine($"Error at point {ownerClaimer.getLine(offset)}:{ownerClaimer.getChar(offset)} \"{raw}\"\n{e.Message}");
            // This should theoretically climb UP the call stack with the error untill something (Or nothing) catches it.
            throw new System.Exception($"at {ownerClaimer.runnerFile}:{ownerClaimer.getLine(offset)}:{ownerClaimer.getChar(offset)} \"{raw.Split('\n')[0] + (raw.IndexOf("\n")>-1?"...":"")}\"\n{e.Message}");
        }

        public abstract void TokenToBinary(BinaryWriter writer);
        public abstract Token BinaryToToken(BinaryReader reader);
    }
}