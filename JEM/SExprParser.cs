using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sprache;


namespace JEM
{
    public static class SExprParser
    {
        public static Parser<Symbol> Symbol = 
            Parse
            .AnyChar
            .Except(Parse.WhiteSpace.Or(Parse.Char('(')).Or(Parse.Char(')')))
            .Many().Text()
            .Select(str => new JEM.Symbol(str));

        public static Parser<SExpr> SExpr(Parser<IEnumerable<Value>> content)
        {
            return from lt in Parse.Char('(')
                   from inners in content
                   from gt in Parse.Char(')').Token()
                   select new SExpr(inners.ToList());
        }

        public static Parser<Value> Value = Symbol.XOr<Value>(Parse.Ref(() => SExpr(Values)));

        public static Parser<IEnumerable<Value>> Symbols = Symbol.DelimitedBy(Parse.WhiteSpace);

        public static Parser<IEnumerable<Value>> Values = Symbols.Or(Value.Many());

        public static Value ParseSExpr(string input)
        {
            //return Symbol.Parse(input);
            return new SExpr(Values.Parse(input).ToList());
        }
        
    }
}
