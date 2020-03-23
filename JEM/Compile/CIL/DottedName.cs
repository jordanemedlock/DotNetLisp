using System;
using JEM.Model;

namespace JEM.Compile.CIL
{
    internal class DottedName : ITransformer<Expr, string>
    {
        public DottedName()
        {
        }

        public bool MatchesPattern(Expr input)
        {
            return input.Is<Symbol>();
        }

        public string Transform(Expr expr)
        {
            return expr.As<Symbol>().Value;
        }
    }
}