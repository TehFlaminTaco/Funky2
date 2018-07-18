using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace Funky{

    class StringClaimer{
        string to_claim;
        Stack<ClaimLoc> prev_claims = new Stack<ClaimLoc>();

        static Regex whitespace = new Regex(@"^(\$\*([^*]|\*[^$])*\*\$|\$[^*\r\n].*|\s)*");

        public bool wsignored = true;
        int offset = 0;
        public StringClaimer(string text){
            to_claim = text;
        }

        public int Location(){
            return offset;
        }

        public string SubString(int a, int b){
            return to_claim.Substring(a, b - a);
        }

        public string SubString(int a){
            return SubString(a, offset);
        }

        public Claim Claim(Regex method){
            if(wsignored)
                offset += whitespace.Match(to_claim.Substring(offset)).Length;

            Match string_match = method.Match(to_claim.Substring(offset));
            if((!string_match.Success) || string_match.Index > 0){ // Incase I forget a ^ somewhere. Try not to forget a ^ somewhere, please taco.
                return new Claim();
            }
            Claim newClaim = new Claim(method, string_match, this);
            prev_claims.Push(new ClaimLoc(newClaim, offset));
            offset += string_match.Length;
            return newClaim;
        }

        public Claim failPoint(){
            if(wsignored)
                offset += whitespace.Match(to_claim.Substring(offset)).Length;
            
            Claim c = new Claim(null, null, this);
            c.success = true;
            prev_claims.Push(new ClaimLoc(c, offset));
            return c;
        }

        public bool Revert(Claim claim){
            Stack<ClaimLoc> storeStack = new Stack<ClaimLoc>(prev_claims);
            while(prev_claims.Count > 0){
                ClaimLoc top = storeStack.Pop();
                if(top.claim == claim){
                    offset = top.location;
                    return true;
                }
            }
            prev_claims = storeStack;
            return false;
        }

        public bool PopTo(Claim claim){
            Stack<ClaimLoc> storeStack = new Stack<ClaimLoc>(prev_claims);
            while(prev_claims.Count > 0){
                ClaimLoc top = storeStack.Pop();
                if(top.claim == claim){
                    return true;
                }
            }
            prev_claims = storeStack;
            return false;
        }

    }

    struct ClaimLoc{
        public Claim claim;
        public int location;
        public ClaimLoc(Claim claim, int location){
            this.claim = claim;
            this.location = location;
        }
    }

    class Claim{
        public bool success = false;
        private Regex claimMethod;
        private Match match;
        StringClaimer claimer;

        public Claim(Regex claimMethod, Match match, StringClaimer claimer){
            success = true;
            this.claimMethod = claimMethod;
            this.match = match;
            this.claimer = claimer;
        }

        public Claim(){}

        /// <exception cref="Funky.FailedClaimException">Throws a Failed Claim Exception if the claim didn't succeed.</exception>
        public string GetText(){
            if(!success)
                throw new FailedClaimException();
            return match.Value;
        }

                /// <exception cref="Funky.FailedClaimException">Throws a Failed Claim Exception if the claim didn't succeed.</exception>
        public Match GetMatch(){
            if(!success)
                throw new FailedClaimException();
            return match;
        }

        /// <exception cref="Funky.FailedClaimException">Throws a Failed Claim Exception if the claim didn't succeed.</exception>
        public Regex GetMethod(){
            if(!success)
                throw new FailedClaimException();
            return claimMethod;
        }

        /// <exception cref="Funky.FailedClaimException">Throws a Failed Claim Exception if the claim didn't succeed.</exception>
        public bool Pass(){
            if(!success)
                throw new FailedClaimException();
            return claimer.PopTo(this);
        }

        /// <exception cref="Funky.FailedClaimException">Throws a Failed Claim Exception if the claim didn't succeed.</exception>
        public bool Fail(){
            if(!success)
                throw new FailedClaimException();
            return claimer.Revert(this);
        }
    }

    public class FailedClaimException : System.Exception
    {
        public FailedClaimException() { }
        public FailedClaimException(string message) : base(message) { }
        public FailedClaimException(string message, System.Exception inner) : base(message, inner) { }
        protected FailedClaimException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}