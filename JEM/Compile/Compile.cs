using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using JEM.Model;

namespace JEM.Compile
{
    public class Compile
    {


        public static Compiler<T, U> Return<T, U>(U output)
        {
            return input =>
            {
                return new List<CompilerResult<T, U>>()
                {
                    new CompilerResult<T, U>()
                    {
                        Value = output,
                        Remainder = input
                    }
                };
            };
        }

    }
}
