
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Funky.Tokens{
    abstract class Token : IBinaryReadWritable{

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

        public static Dictionary<char, Type> tokensById;
        public static Token Make(BinaryReader reader){
            var typ = reader.ReadByte();
            if(typ == '\x00')return null;
            if(tokensById is null){
                tokensById=Assembly.GetExecutingAssembly().GetTypes().Select(c=>(c, c.GetCustomAttribute(typeof(TokenIdentifier)) as TokenIdentifier)).Where(c=>c.Item2!=null).ToDictionary(c=>c.Item2.id, c=>c.Item1);
            }
            Token tok = (Token)Activator.CreateInstance(tokensById[(char)typ]);
            return (Token)tok.Read(reader);
        }

        public object Read(BinaryReader reader)
        {
            foreach ((var field, var options) in this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(c=>(c, c.GetCustomAttribute(typeof(InBinary)) as InBinary)).Where(c=>!(c.Item2 is null))){
                if(IBinaryReadWritable.CanGet(field.FieldType)){
                    field.SetValue(this, IBinaryReadWritable.Make(reader, field.FieldType));
                }else{
                    throw new ArgumentException($"InBinary assigned to field {field.Name} of {this.GetType()}, but it isn't IBinaryReadWritable!");
                }
            }
            return this;
        }

        public void Write(BinaryWriter writer)
        {
            var identifier = (this.GetType().GetCustomAttribute(typeof(TokenIdentifier)) as TokenIdentifier)??throw new ArgumentException($"Expected Token Identifier, None assigned for type {this.GetType()}");
            writer.Write(identifier.id);
            foreach ((var field, var options) in this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(c=>(c, c.GetCustomAttribute(typeof(InBinary)) as InBinary)).Where(c=>!(c.Item2 is null))){
                if(IBinaryReadWritable.CanGet(field.FieldType)){
                    var toWrite = IBinaryReadWritable.Get(field.FieldType, field.GetValue(this));
                    if(toWrite == null){
                        if(options.optional == false){
                            throw new ArgumentException($"Tried to write null field {field.FieldType} {field.Name}, but wasn't optional!");
                        }else{
                            writer.Write('\x00');
                        }
                    }else{
                        toWrite.Write(writer);
                    }
                }else{
                    throw new ArgumentException($"InBinary assigned to field {field.FieldType} {field.Name} of {this.GetType()}, but it isn't IBinaryReadWritable!");
                }
            }
        }
    }
}