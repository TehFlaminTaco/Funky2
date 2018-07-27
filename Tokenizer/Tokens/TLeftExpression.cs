namespace Funky.Tokens{
    enum Associativity{
        NA,
        LEFT_TO_RIGHT,
        RIGHT_TO_LEFT
    }

    // "Left Expressions" are tokens that use an expression as their first argument.
    // They require a "GetLeft" method, which returns their leftmost expression, and a "SetLeft" method, to overrite it.
    // They should also provide a "GetPrecedence" function, which returns an int representing it's Operator Precedence. Lower numbers are tighter grouped than higher numbers. Eg, + might have a precedence of 1, whilst * has a precedence of 2. (Not neccesarily true, just an example.)
    // When in doubt, this function should return -1, to symbolize to always have priority. (Nothing should have a precedence higher than -1)
    // leftClaim is what's called when this is attempted to be claimed. It passes the Expression being used.
    abstract class TLeftExpression : TExpression{
        public abstract TExpression GetLeft();
        public abstract void SetLeft(TExpression newLeft);
        public abstract int GetPrecedence();

        public abstract Associativity GetAssociativity();

        public static TLeftExpression LeftClaim(StringClaimer claimer, TExpression left){return null;}

        new public static TLeftExpression Claim(StringClaimer claimer){
            Claim failTo = claimer.failPoint();
            TExpression newLeft = TExpression.Claim(claimer);
            if(newLeft == null){
                failTo.Fail();
                return null;
            }
            newLeft = LeftClaim(claimer, newLeft);
            if(newLeft == null || !(newLeft is TLeftExpression)){
                failTo.Fail();
                return null;
            }
            return newLeft as TLeftExpression;
        }

        public LeftSteal StealLeft(int precedence, Associativity assoc, TExpression stealer){
            if(GetAssociativity() == Associativity.NA){
                return new LeftSteal(stealer, this);
            }
            int my_prec = GetPrecedence();
            if(precedence < my_prec || (precedence == my_prec && assoc != Associativity.RIGHT_TO_LEFT)){
                TExpression leftExp = GetLeft();
                if(leftExp is TLeftExpression l){
                    LeftSteal stole = l.StealLeft(precedence, assoc, stealer);
                    SetLeft(stole.returnExp);
                    stole.returnExp = this;
                    return stole;
                }
                SetLeft(stealer);
                return new LeftSteal(this, leftExp);
            }else{
                return new LeftSteal(stealer, this);
            }
        }
    }
    struct LeftSteal{
        public TExpression returnExp;
        public TExpression rightExp;
        public LeftSteal(TExpression returnExp, TExpression rightExp){
            this.returnExp = returnExp;
            this.rightExp = rightExp;
        }
    }
}