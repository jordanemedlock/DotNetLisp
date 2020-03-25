using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JEM.Model;

namespace JEM.Compile
{
    public static class Util
    {

        public static Compiler<Expr, Expr> Expr = input =>
        {
            return new List<CompilerResult<Expr, Expr>>()
            {
                new CompilerResult<Expr, Expr>()
                {
                    Value = input,
                    Remainder = null
                }
            };
        };

        public static Compiler<Expr, string> SymbolIs(string value) => Symbol.Where(x => x == value);

        public static Compiler<Expr, string> Symbol = Expr.Is<Expr, Expr, Symbol>().Value();
        public static Compiler<Expr, string> StringConstant = Expr.Is<Expr, Expr, StringConstant>().Value();
        public static Compiler<Expr, long> IntConstant = Expr.Is<Expr, Expr, IntConstant>().Value();
        public static Compiler<Expr, double> FloatConstant = Expr.Is<Expr, Expr, FloatConstant>().Value();
        public static Compiler<Expr, bool> BoolConstant = Expr.Is<Expr, Expr, BoolConstant>().Value();
        public static Compiler<Expr, object> NullConstant = Expr.Is<Expr, Expr, NullConstant>().Value();

        public static Compiler<Expr, string> Value(this Compiler<Expr, Symbol> compiler)
        {
            return compiler.Select(x => x.Value);
        }
        public static Compiler<Expr, string> Value(this Compiler<Expr, StringConstant> compiler)
        {
            return compiler.Select(x => x.Value);
        }
        public static Compiler<Expr, long> Value(this Compiler<Expr, IntConstant> compiler)
        {
            return compiler.Select(x => x.Value);
        }
        public static Compiler<Expr, double> Value(this Compiler<Expr, FloatConstant> compiler)
        {
            return compiler.Select(x => x.Value);
        }
        public static Compiler<Expr, bool> Value(this Compiler<Expr, BoolConstant> compiler)
        {
            return compiler.Select(x => x.Value);
        }
        public static Compiler<Expr, object> Value(this Compiler<Expr, NullConstant> compiler)
        {
            return compiler.Select<Expr, NullConstant, object>(x => null);
        }
        


        
        public static Compiler<Expr, T> Next<T>(Compiler<Expr, T> match)
        {
            return input =>
            {
                if (input is SExpr e && e.Count > 0)
                {
                    var results = match(e.Head());
                    return results.Select(res =>
                    {
                        return new CompilerResult<Expr, T>()
                        {
                            Value = res.Value,
                            Remainder = e.Tail()
                        };
                    }).ToList();
                }
                else
                {
                    throw new PatternMatchException(input, "SExpr with Count > 1");
                }
            };
        }

    }
    
}
