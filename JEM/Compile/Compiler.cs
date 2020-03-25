using System;
using System.Collections.Generic;
using System.Text;
using JEM.Model;

namespace JEM.Compile
{
    public delegate List<CompilerResult<TInput, TOutput>> Compiler<TInput, TOutput>(TInput input);
}
