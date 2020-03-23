using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JEM.Model;
using Superpower;
using Superpower.Parsers;

namespace JEM.Parse
{
    public static class SExprParser
    {
        public static TokenListParser<SExprToken, Value> Symbol = Token
            .EqualTo(SExprToken.Symbol)
            .Apply(Character.AnyChar.Many().Select(x => (Value)new Symbol(new string(x))));

        public static TokenListParser<SExprToken, Value> SExpr =
            from open in Token.EqualTo(SExprToken.Open)
            from expr in Superpower.Parse.Ref(() => Values)
            from close in Token.EqualTo(SExprToken.Close)
            select (Value)new SExpr(expr);

        public static TokenListParser<SExprToken, Value> Value = Symbol.Or(SExpr);

        public static TokenListParser<SExprToken, List<Value>> Values = Value.Many().Select(x => x.ToList());
        

    }
}
