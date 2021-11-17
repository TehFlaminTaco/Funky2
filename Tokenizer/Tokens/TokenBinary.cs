using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Funky.Tokens{
    [AttributeUsage(AttributeTargets.Class)]
    public class TokenIdentifier : Attribute {
        public char id;
        public TokenIdentifier(char id){
            this.id = id;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InBinary : Attribute {
        public bool optional = true;
        public InBinary(bool optional = true){
            this.optional = optional;
        }
    }

    interface IBinaryReadWritable {
        public static int BINARYVERSION = 1;
        public object Read(System.IO.BinaryReader reader);
        public void Write(System.IO.BinaryWriter writer);

        public static List<TExpression> WriteProgram(string code, BinaryWriter writer, string filename="_compiler"){
            List<TExpression> expressions = new List<TExpression>();
            TExpression e;
            StringClaimer claimer = new StringClaimer(code, filename);
            while((e=TExpression.Claim(claimer))!=null){
                claimer.Claim(Executor.SEMI_COLON);
                expressions.Add(e);
                e.Write(writer);
            }
            return expressions;
        }
        public static List<TExpression> ReadProgram(BinaryReader reader){
            List<TExpression> expressions = new List<TExpression>();
            while(reader.PeekChar()!=-1){
                expressions.Add((TExpression)Token.Make(reader));
            }
            return expressions;
        }

        public static void WriteHeader(BinaryWriter writer){
            writer.Write(new char[]{'F','N','K'});
            writer.Write(BINARYVERSION);
        }

        public static bool ReadHeader(BinaryReader reader){
            char[] magicNumbers = reader.ReadChars(3);
            if(magicNumbers.Length < 3)throw new ArgumentException("Not a valid Compiled Funky File");
            if(magicNumbers[0] != 'F' || magicNumbers[1] != 'N' || magicNumbers[2] != 'K'){
                if(Executor.DYNAMIC_RECOMPILE)return false;
                throw new ArgumentException("Not a valid Compiled Funky File");
            }
            int numCheck = reader.ReadInt32();
            if(numCheck < BINARYVERSION){
                if(Executor.DYNAMIC_RECOMPILE)return false;
                throw new ArgumentException("Binary compiled with an older version of Funky. Please recompile.");
            }else if(numCheck > BINARYVERSION){
                if(Executor.DYNAMIC_RECOMPILE)return false;
                throw new ArgumentException("Binary compiled with an newer version of Funky. Please update, or recompile.");
            }
            return true;
        }

        public static IBinaryReadWritable Get(Type t, object obj){
            if(typeof(IBinaryReadWritable).IsAssignableFrom(t)){
                return (IBinaryReadWritable)obj;
            }
            if(typeof(string).IsAssignableFrom(t)){
                return new StringBinaryReadWriter{
                    held = (string)obj
                };
            }
            if(typeof(bool).IsAssignableFrom(t)){
                return new BoolBinaryReadWriter{
                    held = (bool)obj
                };
            }
            if(typeof(double).IsAssignableFrom(t)){
                return new DoubleBinaryReadWriter{
                    held = (double)obj
                };
            }
            if(t.IsEnum){
                Type ltype = typeof(EnumBinaryReadWriter<>).MakeGenericType(t);
                object o = Activator.CreateInstance(ltype);
                ltype.GetField("held").SetValue(o, obj);
                return (IBinaryReadWritable)o;
            }
            if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)){
                Type ltype = typeof(ListBinaryReadWriter<>).MakeGenericType(t.GenericTypeArguments[0]);
                object o = Activator.CreateInstance(ltype);
                ltype.GetField("held").SetValue(o, obj);
                return (IBinaryReadWritable)o;
            }
            return null;
        }

        public static Type GetBinaryReadWriterType(Type t){
            if(typeof(IBinaryReadWritable).IsAssignableFrom(t)){
                return t;
            }
            if(typeof(string).IsAssignableFrom(t)){
                return typeof(StringBinaryReadWriter);
            }
            if(typeof(bool).IsAssignableFrom(t)){
                return typeof(BoolBinaryReadWriter);
            }
            if(typeof(double).IsAssignableFrom(t)){
                return typeof(DoubleBinaryReadWriter);
            }
            if(t.IsEnum){
                return typeof(EnumBinaryReadWriter<>).MakeGenericType(t);
            }
            if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)){
                return typeof(ListBinaryReadWriter<>).MakeGenericType(t.GenericTypeArguments[0]);
            }
            return null;
        }

        public static bool CanGet(Type t){
            if(typeof(IBinaryReadWritable).IsAssignableFrom(t)){
                return true;
            }
            if(t == typeof(System.String) || t == typeof(System.Boolean) || t == typeof(System.Double)){
                return true;
            }
            if(t.IsEnum){
                return true;
            }
            if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)){
                return CanGet(t.GenericTypeArguments[0]);
            }
            return false;
        }

        public static object Make(BinaryReader r, Type t){
            if(IBinaryReadWritable.CanGet(t)){
                var readerMethod = t.GetMethod("Make", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new Type[]{typeof(BinaryReader)}, null);
                if(readerMethod is null){
                    var rwType = GetBinaryReadWriterType(t);
                    readerMethod = rwType.GetMethod("Make", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new Type[]{typeof(BinaryReader)}, null);
                    if(readerMethod is null){
                        IBinaryReadWritable inst = IBinaryReadWritable.Get(t, Activator.CreateInstance(t));
                        return inst.Read(r);
                    }else{
                        return readerMethod.Invoke(null, new object[] {r});
                    }
                }else{
                    return readerMethod.Invoke(null, new object[] {r});
                }
            }else{
                throw new ArgumentException($"Tried to make non-readwritable {t}");
            }
        }
    }

    public class ListBinaryReadWriter<T> : IBinaryReadWritable {
        public List<T> held;

        public object Read(BinaryReader reader)
        {
            held ??= new List<T>();
            held.Clear();
            int count = reader.ReadInt32();
            for(int i = 0; i < count; i++){
                held.Add((T)IBinaryReadWritable.Make(reader, typeof(T)));
            }
            return held;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(held.Count);
            for(int i = 0; i < held.Count; i++){
                var rw = IBinaryReadWritable.Get(typeof(T), held[i]);
                if(rw == null){
                    writer.Write('\x00');    
                }else{
                    rw.Write(writer);
                }
            }
        }
    }

    public class EnumBinaryReadWriter<T> : IBinaryReadWritable where T : Enum {
        public T held;
        public object Read(BinaryReader reader)
        {
            int id = reader.ReadInt32();
            return held=(T)Enum.Parse(typeof(T), $"{id}");
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Int32.Parse(Enum.Format(typeof(T), held, "d")));
        }

        public static T Make(BinaryReader reader){
            return (T)Enum.Parse(typeof(T), $"{reader.ReadInt32()}");
        }
    }

    public class StringBinaryReadWriter : IBinaryReadWritable {
        public string held;

        public object Read(BinaryReader reader)
        {
            held = reader.ReadString();
            return held;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(held);
        }

        public static string Make(BinaryReader reader){
            return reader.ReadString();
        }
    }

    public class BoolBinaryReadWriter : IBinaryReadWritable {
        public bool held;

        public object Read(BinaryReader reader)
        {
            held = reader.ReadBoolean();
            return held;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(held);
        }

        public static bool Make(BinaryReader reader){
            return reader.ReadBoolean();
        }
    }

    public class DoubleBinaryReadWriter : IBinaryReadWritable {
        public double held;

        public object Read(BinaryReader reader)
        {
            held = reader.ReadDouble();
            return held;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(held);
        }

        public static double Make(BinaryReader reader){
            return reader.ReadDouble();
        }
    }
}