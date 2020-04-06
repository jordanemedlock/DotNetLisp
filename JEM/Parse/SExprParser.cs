using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JEM.Model;
using Superpower;
using Superpower.Parsers;

namespace JEM.Parse
{
    public static class SExprParser
    { 

        public static TextParser<Expr> SymbolParser = SExprTokenizer.SymbolToken.Select(x =>
        {
            switch (x.ToStringValue())
            {
                case "null":
                    return (Expr)new NullConstant();
                case "true":
                    return (Expr)new BoolConstant(true);
                case "false":
                    return (Expr)new BoolConstant(false);
                default:
                    return (Expr)new Symbol(x.ToStringValue());
            }
        });
        public static TokenListParser<SExprToken, Expr> SymbolTokenParser = Token
            .EqualTo(SExprToken.Symbol)
            .Apply(SymbolParser);

        



        public static TextParser<Expr> StringParser =
            from open in Character.EqualTo('"')
            from chars in Character.ExceptIn('"', '\\')
                .Or(Character.EqualTo('\\')
                    .IgnoreThen(
                        Character.EqualTo('\\')
                        .Or(Character.EqualTo('"'))
                        .Or(Character.EqualTo('/'))
                        .Or(Character.EqualTo('b').Value('\b'))
                        .Or(Character.EqualTo('f').Value('\f'))
                        .Or(Character.EqualTo('n').Value('\n'))
                        .Or(Character.EqualTo('r').Value('\r'))
                        .Or(Character.EqualTo('t').Value('\t'))
                        .Or(Character.EqualTo('u').IgnoreThen(
                                Span.MatchedBy(Character.HexDigit.Repeat(4))
                                    .Apply(Numerics.HexDigitsUInt32)
                                    .Select(cc => (char)cc)))
                        .Named("escape sequence")))
                .Many()
            from close in Character.EqualTo('"')
            select (Expr)new StringConstant(new string(chars));
        public static TokenListParser<SExprToken, Expr> StringTokenParser = Token
            .EqualTo(SExprToken.String)
            .Apply(StringParser);

        public static TextParser<Expr> IntParser = 
            SExprTokenizer.IntToken.Select(x => (Expr)new IntConstant(long.Parse(x.ToStringValue())));
        public static TokenListParser<SExprToken, Expr> IntTokenParser = Token
            .EqualTo(SExprToken.Integer)
            .Apply(IntParser);

        public static TextParser<Expr> FloatParser = 
            SExprTokenizer.FloatToken
            .Select(x => (Expr)new FloatConstant(double.Parse(x.ToStringValue())));

        public static TokenListParser<SExprToken, Expr> FloatTokenParser = Token
            .EqualTo(SExprToken.Float)
            .Apply(FloatParser);

        public static TokenListParser<SExprToken, Expr> SExpr =
            from open in Token.EqualTo(SExprToken.Open)
            from expr in Superpower.Parse.Ref(() => Exprs)
            from close in Token.EqualTo(SExprToken.Close)
            select (Expr)new SExpr(expr);

        public static TokenListParser<SExprToken, Expr> Expr = 
            SymbolTokenParser
            .Or(StringTokenParser)
            .Or(IntTokenParser)
            .Or(FloatTokenParser)
            .Or(SExpr);

        public static TokenListParser<SExprToken, List<Expr>> Exprs = 
            Expr.Many().Select(x => x.ToList());
        
        public static SExpr Parse(string input)
        {
            var tokenizer = SExprTokenizer.Instance;
            var tokens = tokenizer.Tokenize(input);
            var values = Exprs.Parse(tokens);
            return new SExpr(values);
        }

        public static SExpr ParseFile(string filePath)
        {
            string fileContents = File.ReadAllText(filePath);
            return Parse(fileContents);
        }
    }
}
