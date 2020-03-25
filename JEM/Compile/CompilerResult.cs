using System;
using System.Collections.Generic;
using System.Text;
using JEM.Model;

namespace JEM.Compile
{
    public class CompilerResult<TInput, TOutput>
    {
        public TOutput Value { get; set; }

        public TInput Remainder { get; set; }

        public CompilerResult(TOutput value, TInput remainder)
        {
            Value = value;
            Remainder = remainder;
        }

        public CompilerResult() { }
    }
}
