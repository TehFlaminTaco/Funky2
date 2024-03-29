using System.Text.RegularExpressions;
using System.Globalization;
using System;

namespace Funky.Tokens.Literal{
    [TokenIdentifier('\x0F')]
    class TLiteralNumber : TLiteral{
        [InBinary]VarNumber value;
        static Regex NUMBER = new Regex(@"^(?<negative>-?)(?:(?<integer>0(?:x(?<hex_val>[0-9A-Fa-f]+)|b(?<bin_val>[01]+)))|(?:(?<float>(?<int_comp>\d*)\.(?<float_comp>\d+))|(?<int>\d+))(?:e(?<expon>-?\d+))?)");
        new public static TLiteralNumber Claim(StringClaimer claimer){
            TLiteralNumber numb = new TLiteralNumber();

            Claim claim = claimer.Claim(NUMBER);

            if(!claim.success){
                return null;
            }
            claim.Pass();

            double v = 0.0d;
            Match m = claim.GetMatch();


            if(m.Groups["integer"].Length > 0){ // x or b integer format.
                if(m.Groups["hex_val"].Length > 0){
                    v = (double)int.Parse(m.Groups["hex_val"].Value, NumberStyles.HexNumber);
                }else{
                    v = (double)Convert.ToInt32(m.Groups["bin_val"].Value, 2);
                }
            }else{
                string num;
                if(m.Groups["int"].Length > 0){
                    num = m.Groups["int"].Value;
                }else{
                    num = m.Groups["float"].Value;
                }
                v = Convert.ToDouble(num);
                if(m.Groups["expon"].Length > 0){
                    for(int i = Convert.ToInt32(m.Groups["expon"].Value); i>0; i--)
                        v *= 10;
                }
            }

            if(m.Groups["negative"].Length > 0) // Has a -
                v *= -1;
            numb.value = new VarNumber(v);
            return numb;
        }

        override public Var Parse(Scope scope){
            return value;
        }
    }

}