using System;
using System.Collections.Generic;
using System.Text;
using Superpower.Display;
using Superpower.Model;
using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace JEM.Parse
{

    
    public class SExprTokenizer
    {
        protected static TextParser<TextSpan> SymbolToken = Span.MatchedBy(
            from open in Character.Letter.Or(Character.In('.','_','<','>','[',']','{','}'))
            from content in Character.Except(x => " ()".Contains(x), "symbol characters").Many()
            select open + new string(content));

        static TextParser<Unit> StringToken { get; } =
            from open in Character.EqualTo('"')
            from content in Span.EqualTo("\\\"").Value(Unit.Value).Try()
                .Or(Character.Except('"').Value(Unit.Value))
                .IgnoreMany()
            from close in Character.EqualTo('"')
            select Unit.Value;

        static TextParser<TextSpan> IntToken = Numerics.Integer;

        static TextParser<TextSpan> FloatToken = Numerics.Decimal;

        public static Tokenizer<SExprToken> Instance { get; } =
            new TokenizerBuilder<SExprToken>()
                .Ignore(Span.WhiteSpace)
                .Match(Character.EqualTo('('), SExprToken.Open)
                .Match(Character.EqualTo(')'), SExprToken.Close)
                .Match(SymbolToken, SExprToken.Symbol, requireDelimiters: true)
                .Match(StringToken, SExprToken.String, requireDelimiters: true)
                .Match(IntToken, SExprToken.Integer, requireDelimiters: true)
                .Match(FloatToken, SExprToken.Float, requireDelimiters: true)
                .Build();
        
        
    }

}
