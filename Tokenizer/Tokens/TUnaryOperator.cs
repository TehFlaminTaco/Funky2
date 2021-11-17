using System.IO;
using System.Text.RegularExpressions;
namespace Funky.Tokens{

    [TokenIdentifier('\x1F')]
    class TUnaryOperator : Token{
        public static SUnOperator[] operators = {
            new SUnOperator(@"\+", "unp", 4),
            new SUnOperator(@"-", "unm", 4),
            new SUnOperator(@"!", "not", 4),
            new SUnOperator(@"not", "not", 17),
            new SUnOperator(@"#", "len", 4),
            new SUnOperator(@"~", "bitnot", 4)
        };
        [InBinary(optional = false)]public SUnOperator op;

        new public static TUnaryOperator Claim(StringClaimer claimer){
            for(int i = 0; i < operators.Length; i++){
                SUnOperator thisOp = operators[i];
                Claim c = claimer.Claim(thisOp.regex);
                if(c.success){
                    c.Pass();
                    TUnaryOperator newOp = new TUnaryOperator();
                    newOp.op = thisOp;
                    return newOp;
                }
            }
            return null;
        }

        public Var Parse(Var right){
            Var metaMethod = Meta.Get(right, op.name, $"right={right.type}");
            if(!(metaMethod is VarNull))
                return metaMethod.Call(new CallData(right));
            return Var.nil;
        }
    }

    struct SUnOperator : IBinaryReadWritable{
        public Regex regex;
        public string name;
        public int precedence;

        public SUnOperator(string regex, string name, int precedence){
            this.regex = new Regex("^"+regex);
            this.name = name;
            this.precedence = precedence;
        }
        public object Read(BinaryReader reader)
        {
            int id = reader.ReadInt32();
            var op=TUnaryOperator.operators[id];
            regex = op.regex;
            name = op.name;
            precedence = op.precedence;
            return op;
        }

        public void Write(BinaryWriter writer)
        {
            string nm = name;
            writer.Write(System.Array.FindIndex(TUnaryOperator.operators, c=>c.name==nm));
        }

        public static SUnOperator Make(BinaryReader reader){
            int id = reader.ReadInt32();
            return TUnaryOperator.operators[id];
        }
    }
}