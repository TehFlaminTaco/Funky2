using System.Text.RegularExpressions;
using System.Text;
namespace Funky.Tokens{

    // Lower Operator Precedence is "More Sticky".
    class TOperator : Token{

        private static SOperator[] operators = {
            new SOperator(@"\+", "add", 7),
            new SOperator(@"-", "sub", 7),
            new SOperator(@"\*", "mult", 6),
            new SOperator(@"//", "intdiv", 6),
            new SOperator(@"/", "div", 6),
            new SOperator(@"\^", "pow", 5,  Associativity.RIGHT_TO_LEFT),
            new SOperator(@"\%", "mod", 6),
            new SOperator(@"&&", "and", 15),
            new SOperator(@"\|\|", "or", 16),
            new SOperator(@"and", "and", 15),
            new SOperator(@"or", "or", 16),
            new SOperator(@"\.\.", "concat", 8),
            new SOperator(@"\|", "bitor", 14),
            new SOperator(@"&", "bitand", 12),
            new SOperator(@"~", "bitxor", 13),
            new SOperator(@"<<", "bitshiftl", 9),
            new SOperator(@">>", "bitshiftr", 9),
            new SOperator(@"<", "lt", 10),
            new SOperator(@"<=", "le", 10),
            new SOperator(@">", "gt", 10),
            new SOperator(@">=", "ge", 10),
            new SOperator(@"==", "eq", 11),
            new SOperator(@"!=", "ne", 11)
        };

        public SOperator op;
        public int GetPrecedence(){
            return op.precedence;
        }

        public Associativity GetAssociativity(){
            return op.associativity;
        }

        new public static TOperator Claim(StringClaimer claimer){
            for(int i = 0; i < operators.Length; i++){
                SOperator thisOp = operators[i];
                Claim c = claimer.Claim(thisOp.regex);
                if(c.success){
                    c.Pass();
                    TOperator newOp = new TOperator();
                    newOp.op = thisOp;
                    return newOp;
                }
            }
            return null;
        }

        public Var Parse(Scope scope, TExpression left, TExpression right){
            Var l = Var.nil;
            if(op.name == "and"){
                l = left.Parse(scope);
                if(!l.asBool()){
                    return l;
                }
                return right.Parse(scope);
            }
            if(op.name == "or"){
                l = left.Parse(scope);
                if(l.asBool()){
                    return l;
                }
                return right.Parse(scope);
            }
            return Parse(left.Parse(scope), right.Parse(scope));
        }

        public Var Parse(Var left, Var right){
            if(op.name == "and"){
                if(!left.asBool()){
                    return left;
                }
                return right;
            }
            if(op.name == "or"){
                if(left.asBool()){
                    return left;
                }
                return right;
            }
            Var metaMethod = Meta.LR_Get(left, right, op.name, $"left={left.type}", $"right={right.type}");
            if(!(metaMethod is VarNull))
                return metaMethod.Call(new CallData(left, right));
            return Var.nil;
        }
    }

    struct SOperator{
        public Regex regex;
        public string name;
        public int precedence;

        public Associativity associativity;
        public SOperator(string regex, string name, int precedence){
            this.regex = new Regex(regex);
            this.name = name;
            this.precedence = precedence;
            this.associativity = Associativity.LEFT_TO_RIGHT;
        }

        public SOperator(string regex, string name, int precedence, Associativity assoc){
            this.regex = new Regex(regex);
            this.name = name;
            this.precedence = precedence;
            this.associativity = assoc;
        }
    }
}