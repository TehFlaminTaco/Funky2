using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using System;

namespace Funky.Tokens.Literal{
    abstract class TLiteral : TExpression{
        new public static TLiteral Claim(StringClaimer claimer){
            return TLiteralNumber.Claim(claimer)    as TLiteral ??
            TLiteralString.Claim(claimer)           as TLiteral ??
            TLiteralStringTemplate.Claim(claimer)   as TLiteral ??
            TLiteralList.Claim(claimer)             as TLiteral;
        }
    }
}