using System;
using System.Collections.Generic;
using System.Text;
using JEM.Model;

namespace JEM.Compile
{
    public delegate CompilerResult<TInput, TOutput> Compiler<TInput, TOutput>(TInput input);
}
