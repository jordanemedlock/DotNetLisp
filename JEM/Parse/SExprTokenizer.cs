﻿using System;
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
        public static TextParser<TextSpan> SymbolToken = Span.MatchedBy(
            from open in Character.Letter.Or(Character.Except(x => " \"'()".Contains(x), "symbol characters"))
            from content in Character.Except(x => " ()".Contains(x), "symbol characters").Many()
            select open + new string(content));

        static TextParser<Unit> StringToken { get; } =
            from open in Character.EqualTo('"')
            from content in Span.EqualTo("\\\"").Value(Unit.Value).Try()
                .Or(Character.Except('"').Value(Unit.Value))
                .IgnoreMany()
            from close in Character.EqualTo('"')
            select Unit.Value;

        public static TextParser<TextSpan> IntToken = Numerics.Integer;

        static TextParser<TextSpan> DecimalToken = 
            Numerics.Integer
                .Then(n => 
                    Character.EqualTo('.').IgnoreThen(Numerics.Natural).OptionalOrDefault()
                    .Select(f => f == TextSpan.None ? n : new TextSpan(n.Source, n.Position, n.Length + f.Length + 1)));

        public static TextParser<TextSpan> FloatToken =
            DecimalToken.Then(d =>
                Character.In('e', 'E').IgnoreThen(Numerics.Integer).OptionalOrDefault()
                .Select(f => f == TextSpan.None ? d : new TextSpan(d.Source, d.Position, d.Length + f.Length + 1)));

        public static Tokenizer<SExprToken> Instance { get; } =
            new TokenizerBuilder<SExprToken>()
                .Ignore(Span.WhiteSpace)
                .Match(Character.EqualTo('('), SExprToken.Open)
                .Match(Character.EqualTo(')'), SExprToken.Close)
                .Match(IntToken, SExprToken.Integer, requireDelimiters: true)
                .Match(FloatToken, SExprToken.Float, requireDelimiters: true)
                .Match(SymbolToken, SExprToken.Symbol, requireDelimiters: true)
                .Match(StringToken, SExprToken.String, requireDelimiters: true)
                .Build();
        
        
    }

}
