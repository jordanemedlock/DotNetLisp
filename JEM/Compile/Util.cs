using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JEM.Model;

namespace JEM.Compile
{
    public static class Util
    {

        public static Compiler<Expr, Expr> Expr = new Compiler<Expr,Expr>(input =>
        {
            return new CompilerResult<Expr, Expr>(input, null);
        });

        public static Compiler<Expr, string> SymbolIs(string value) => Symbol.Where(x => x == value, $"{{0}} is not Symbol({value})");
        public static Compiler<Expr, string> SymbolIn(params string[] values) => Symbol.Where(x => values.Contains(x), "{0} is not in [" + String.Join(", ", values) + "]");
        public static Compiler<Expr, string> SymbolIn(List<string> values) => Symbol.Where(x => values.Contains(x), "{0} is not in [" + String.Join(", ", values) + "]");

        public static Compiler<Expr, string> Symbol = Expr.Is<Symbol>().Value();
        public static Compiler<Expr, StringConstant> StringConstant = Expr.Is<StringConstant>();
        public static Compiler<Expr, long> IntConstant = Expr.Is<IntConstant>().Value();
        public static Compiler<Expr, double> FloatConstant = Expr.Is<FloatConstant>().Value();
        public static Compiler<Expr, bool> BoolConstant = Expr.Is<BoolConstant>().Value();
        public static Compiler<Expr, object> NullConstant = Expr.Is<NullConstant>().Value();

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
            return compiler.Select<object>(x => null);
        }




        public static Compiler<Expr, T> Next<T>(Compiler<Expr, T> match)
        {
            return new Compiler<Expr, T>($"Next({match.Name})", input =>
            {
                if (input is SExpr e && e.Count > 0)
                {
                    var results = match.Compile(e.Head());
                    return results.Bind(val =>
                    {
                        return new CompilerResult<Expr, T>()
                        {
                            Value = val,
                            Remainder = e.Tail()
                        };
                    });
                }
                else
                {
                    return new CompilerResult<Expr, T>($"{input.ToString()} is not SExpr or emtpy in Next");
                }
            });
        }
        public static Compiler<Expr, string> Next(string symbol)
        {
            return new Compiler<Expr, string> ($"Next({symbol})", input =>
            {
                if (input is SExpr e && e.Count > 0)
                {
                    var results = SymbolIs(symbol).Compile(e.Head());
                    return results.Bind(val =>
                    {
                        return new CompilerResult<Expr, string>()
                        {
                            Value = val,
                            Remainder = e.Tail()
                        };
                    });
                }
                else
                {
                    return new CompilerResult<Expr, string>($"{input.ToString()} is not SExpr or emtpy in NextSymbol");
                }
            });
        }



        // Is ther any way I can make this more complicated lol
        public static Compiler<Expr, List<T>> Many<T>(this Compiler<Expr, T> inner)
        {
            return new Compiler<Expr, List<T>>($"Many({inner.Name})", input =>
            {
                if (input is SExpr e)
                {
                    var results = new CompilerResult<Expr, List<T>>()
                    {
                        Value = new List<T>(),
                        Remainder = null
                    };
                    int i = 0;
                    foreach (var value in e.Values)
                    {
                        var innerResults = inner.Compile(value);
                        if (innerResults.HasValue)
                        {

                            results.Value.Add(innerResults.Value);
                        }
                        else
                        {
                            results.Remainder = new SExpr(e.Values.GetRange(i, (int)e.Count - i));
                            break;
                        }
                        i++;
                    }
                    return results;
                }
                else
                {
                    return new CompilerResult<Expr, List<T>>($"{input} is not SExpr in Many");
                }
            });
        }

        public static Compiler<Expr, List<T>> AtLeastOnce<T>(this Compiler<Expr, T> inner)
        {
            return new Compiler<Expr, List<T>>($"AtLeastOnce({inner.Name})", input =>
            {
                if (input is SExpr e && e.Count >= 1)
                {
                    var results = new CompilerResult<Expr, List<T>>()
                    {
                        Value = new List<T>(),
                        Remainder = null
                    };
                    int i = 0;
                    foreach (var value in e.Values)
                    {
                        var innerResults = inner.Compile(value);
                        if (innerResults.HasValue)
                        {

                            results.Value.Add(innerResults.Value);
                        }
                        else
                        {
                            results.Remainder = new SExpr(e.Values.GetRange(i, (int)e.Count - i));
                            break;
                        }
                        i++;
                    }
                    return results;
                }
                else
                {
                    return new CompilerResult<Expr, List<T>>($"{input} is not SExpr (or Count >=1) in AtLeastOnce");
                }
            });
        }

        internal static Compiler<Expr, T> NextOptional<T>(Compiler<Expr, T> match)
        {

            return new Compiler<Expr, T>($"NextOptional({match.Name})", input =>
            {
                if (input is SExpr e && e.Count > 0)
                {
                    var results = match.Compile(e.Head());
                    if (results.HasValue)
                    {
                        return results.Bind(val =>
                        {
                            return new CompilerResult<Expr, T>()
                            {
                                Value = val,
                                Remainder = e.Tail()
                            };
                        });
                    }
                    else
                    {
                        return new CompilerResult<Expr, T>()
                        {
                            Value = default(T),
                            Remainder = e
                        };
                    }
                }
                else if (input is SExpr)
                {
                    return new CompilerResult<Expr, T>()
                    {
                        Value = default(T),
                        Remainder = input
                    };
                }
                else
                {
                    return new CompilerResult<Expr, T>($"{input.ToString()} is not SExpr in NextOptional");
                }
            });
        }
        public static Compiler<Expr, string> NextOptional(string symbol)
        {
            return NextOptional(SymbolIs(symbol));
        }
    }
    
}
