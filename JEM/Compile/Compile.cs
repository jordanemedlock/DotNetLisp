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
                return new CompilerResult<T, U>(output, input);
            };
        }

        public static Compiler<T, U> Error<T, U>(string message)
        {
            return input =>
            {
                return new CompilerResult<T, U>(message);
            };
        }

    }
}
