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
        public static Compiler<Expr, string> SymbolIn(params string[] values) => Symbol.Where(x => values.Contains(x));
        public static Compiler<Expr, string> SymbolIn(List<string> values) => Symbol.Where(x => values.Contains(x));

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
                    return new List<CompilerResult<Expr, T>>();
                }
            };
        }

        

        // Is ther any way I can make this more complicated lol
        public static Compiler<Expr, List<T>> Many<T>(this Compiler<Expr, T> inner)
        {
            return input =>
            {
                if (input is SExpr e)
                {
                    var results = new List<CompilerResult<Expr, List<T>>>()
                    {
                        new CompilerResult<Expr, List<T>>()
                        {
                            Value = new List<T>(),
                            Remainder = null
                        }
                    };
                    foreach (var value in e.Values)
                    {
                        var innerResults = inner(value);
                        if (innerResults.Count > 0)
                        {

                            results[0].Value.Add(innerResults[0].Value);
                        }
                        else
                        {
                            // we need to pass on the error
                            return new List<CompilerResult<Expr, List<T>>>();
                        }
                    }
                    return results;
                }

                return new List<CompilerResult<Expr, List<T>>>();
            };
        }

        internal static Compiler<Expr,T> NextOptional<T>(Compiler<Expr, T> match)
        {

            return input =>
            {
                if (input is SExpr e && e.Count > 0)
                {
                    var results = match(e.Head());
                    if (results.Count > 0)
                    {
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
                        return new List<CompilerResult<Expr, T>>()
                        {
                            new CompilerResult<Expr, T>()
                            {
                                Value = default(T),
                                Remainder = e
                            }
                        };
                    }
                }
                else if (input is SExpr)
                {
                    return new List<CompilerResult<Expr, T>>()
                    {
                        new CompilerResult<Expr, T>()
                        {
                            Value = default(T),
                            Remainder = input
                        }
                    };
                }
                else
                {
                    return new List<CompilerResult<Expr, T>>();
                }
            };
        }
    }
    
}
