using System;
using System.Collections.Generic;
using System.Text;
using Superpower.Display;
using Superpower.Model;
using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;
using System.Linq;

namespace JEM.Parse
{

    
    public class SExprTokenizer
    {

        public static char[] OperatorCharacters = new char[]{'`','~','!','@','#','$','%','^','&','*','-','=','+','[',']','{','}',';',':',',','<','>','/','?','|','\\'};
        public static TextParser<TextSpan> SymbolToken = Span.MatchedBy(
            from open in Character.Letter.Or(Character.In('_','.'))
            from content in Character.Letter.Or(Character.In('_','.')).Or(Character.Numeric).Many()
            select open + new string(content));

        public static TextParser<TextSpan> OperatorToken = Span.MatchedBy(
            from open in Character.In(OperatorCharacters)
            from rest in Character.In(OperatorCharacters).Many()
            select open + new string(rest)
        );

        public static TextParser<TextSpan> DQStringToken { get; } = Span.MatchedBy(
            from open in Character.EqualTo('"')
            from content in Span.EqualTo("\\\"").Value(Unit.Value).Try()
                .Or(Character.Except('"').Value(Unit.Value))
                .IgnoreMany()
            from close in Character.EqualTo('"')
            select Unit.Value);

        public static TextParser<TextSpan> SQStringToken { get; } = Span.MatchedBy(
            from open in Character.EqualTo('\'')
            from content in Span.EqualTo("\\\'").Value(Unit.Value).Try()
                .Or(Character.Except('\'').Value(Unit.Value))
                .IgnoreMany()
            from close in Character.EqualTo('\'')
            select Unit.Value);

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
                .Match(OperatorToken, SExprToken.Operator)
                .Match(SymbolToken, SExprToken.Symbol, requireDelimiters: true)
                .Match(DQStringToken, SExprToken.DQString, requireDelimiters: true)
                .Match(SQStringToken, SExprToken.SQString, requireDelimiters: true)
                .Build();
        
        
    }

}
