using System;
using System.Collections.Generic;
using System.Text;
using JEM.Model;

namespace JEM.Compile
{
    class PatternMatchException : Exception
    {
        public PatternMatchException(Expr failedExpr, string message) : base($"Pattern match on {failedExpr.ToString()} failed with message: {message}")
        {
           
        }
    }
}
