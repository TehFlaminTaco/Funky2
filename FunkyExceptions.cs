using Funky.Tokens;
using System;

namespace Funky{
    public class FunkyException : Exception{
        public FunkyException(string errorMessage) : base(errorMessage) { }
    }

    public class FunkyArgumentException : FunkyException {
        public FunkyArgumentException(string errorMessage) : base(errorMessage) { }
    }
}