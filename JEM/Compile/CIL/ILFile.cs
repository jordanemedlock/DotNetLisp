using System;
using System.Collections.Generic;
using System.Text;
using JEM.Model;

namespace JEM.Compile.CIL
{
    class ILFile : Compiler<Expr, string>, ITransformer<Expr, string>
    {
        public bool MatchesPattern(Expr input)
        {
            return input.Is<SExpr>(); // top of the line, we dont need to match it.
        }

        public string Transform(Expr input)
        {
            return Compile(input);
        }

        public override string Compile(Expr input)
        {
            var sexpr = input.As<SExpr>();
            var values = new List<string>();
            foreach (var expr in sexpr)
            {
                foreach (var t in Transformers)
                {
                    if (t.MatchesPattern(expr))
                    {
                        values.Add(t.Transform(expr));
                        break;
                    }
                }
            }
            return String.Join('\n', values);
        }

        public override List<ITransformer<Expr, string>> Transformers { get => new List<ITransformer<Expr, string>>()
                {
                    new Assembly()
                };
            set => base.Transformers = value; }

    }
}
