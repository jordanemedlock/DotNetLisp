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

        public static Compiler<Expr, T> Return<T>(T value) 
        {
            return new Compiler<Expr, T>($"=>{value}", input => new CompilerResult<Expr, T>(value, input));
        }

        public static Compiler<Expr, string> SymbolIs(string value) => 
            Symbol.Where(x => x == value, $"{value}");

        public static Compiler<Expr, string> SymbolIn(params string[] values) => 
            Symbol.Where(x => values.Contains(x), String.Join(" | ", values));

        public static Compiler<Expr, string> SymbolIn(List<string> values) => 
            Symbol.Where(x => values.Contains(x), String.Join(" | ", values));


        public static Compiler<Expr, string> OperatorIs(string value) => 
            Operator.Where(x => x == value, $"`{value}`");

        public static Compiler<Expr, string> OperatorIn(params string[] values) => 
            Operator.Where(x => values.Contains(x), String.Join(" | ", values.Select(x => $"`{x}`")));

        public static Compiler<Expr, string> OperatorIn(List<string> values) => 
            Operator.Where(x => values.Contains(x), String.Join(" | ", values.Select(x => $"`{x}`")));

        public static Compiler<Expr, string> Symbol = Expr.Is<Symbol>().Value();
        public static Compiler<Expr, string> Operator = Expr.Is<Operator>().Value();
        public static Compiler<Expr, StringConstant> DQStringConstant = Expr.Is<StringConstant>().Where(s => !s.SingleQuote, "DQString");
        public static Compiler<Expr, StringConstant> SQStringConstant = Expr.Is<StringConstant>().Where(s => s.SingleQuote, "SQString");
        public static Compiler<Expr, long> IntConstant = Expr.Is<IntConstant>().Value();
        public static Compiler<Expr, double> FloatConstant = Expr.Is<FloatConstant>().Value();
        public static Compiler<Expr, bool> BoolConstant = Expr.Is<BoolConstant>().Value();
        public static Compiler<Expr, object> NullConstant = Expr.Is<NullConstant>().Value();

        public static Compiler<Expr, string> Value(this Compiler<Expr, Symbol> compiler)
        {
            return compiler.Select(x => x.Value);
        }
        public static Compiler<Expr, string> Value(this Compiler<Expr, Operator> compiler)
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
            return new Compiler<Expr, T>($"{match.Name}", input =>
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
                    return new CompilerResult<Expr, T>($"{input.ToString()} is not SExpr or emtpy in : {match.Name}");
                }
            });
        }
        public static Compiler<Expr, string> Next(string symbol)
        {
            return new Compiler<Expr, string> ($"{symbol}", input =>
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
                    return new CompilerResult<Expr, string>($"{input.ToString()} is not SExpr or emtpy in : {symbol}");
                }
            });
        }



        // This one fails if any of the inner compilers fail
        public static Compiler<Expr, List<T>> Many<T>(this Compiler<Expr, T> inner)
        {
            return new Compiler<Expr, List<T>>($"{inner.Name}*", input =>
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
                            return new CompilerResult<Expr, List<T>>(innerResults.Error);
                        }
                        i++;
                    }
                    return results;
                }
                else
                {
                    return new CompilerResult<Expr, List<T>>($"{input} is not SExpr in {inner.Name}*");
                }
            });
        }


        // Is ther any way I can make this more complicated lol
        public static Compiler<Expr, List<T>> ManyUntil<T>(this Compiler<Expr, T> inner)
        {
            return new Compiler<Expr, List<T>>($"{inner.Name}*", input =>
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
                    return new CompilerResult<Expr, List<T>>($"{input} is not SExpr in {inner.Name}*");
                }
            });
        }

        public static Compiler<Expr, List<T>> AtLeastOnce<T>(this Compiler<Expr, T> inner)
        {
            return new Compiler<Expr, List<T>>($"{inner.Name}+", input =>
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
                    if (results.Value.Count == 0) {
                        return new CompilerResult<Expr, List<T>>($"{input} is not SExpr (or Count >=1) in {inner.Name}+");
                    }
                    return results;
                }
                else
                {
                    return new CompilerResult<Expr, List<T>>($"{input} is not SExpr (or Count >=1) in {inner.Name}+");
                }
            });
        }

        internal static Compiler<Expr, T> NextOptional<T>(Compiler<Expr, T> match)
        {

            return new Compiler<Expr, T>($"{match.Name}?", input =>
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
                    return new CompilerResult<Expr, T>($"{input.ToString()} is not SExpr in {match.Name}?");
                }
            });
        }

        public static Compiler<Expr, T> StartsWith<T>(Dictionary<string, Compiler<Expr, T>> compilers)
        {
            return new Compiler<Expr, T>(input => 
            {
                string start;
                SExpr rest;
                if (input is SExpr se && se.Count > 0 && se.Head() is Symbol symbol) 
                {
                    start = symbol.Value;
                    rest = se.Tail();
                }
                else if (input is Symbol s)
                {
                    start = s.Value;
                    rest = null;
                }
                else 
                {
                    return new CompilerResult<Expr, T>($"{input} is not SExpr or Symbol in StartsWith");
                }

                foreach (var kvp in compilers)
                {
                    if (start == kvp.Key)
                    {
                        return kvp.Value.Compile(rest);
                    }
                }

                return new CompilerResult<Expr, T>($"{start} is not in {compilers.Keys.ToList()}");
            });
        }

        public static Compiler<Expr, string> NextOptional(string symbol)
        {
            return NextOptional(SymbolIs(symbol));
        }

        internal static Compiler<Expr, string> Nil()
        {
            return Expr.Is<SExpr>().Where(s => s.Count == 0, "Nil").Return("");
        }
    }
    
}
