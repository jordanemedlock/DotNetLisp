using System;
using System.Collections.Generic;
using System.Linq;
using JEM.Parse;
using Superpower;
using Xunit;

namespace JEM.Testing
{
    public class TokenizerTest
    {
        public Tokenizer<SExprToken> Tokenizer { get; } = SExprTokenizer.Instance;

        [Fact]
        public void TestEmptyString()
        {
            var res = Tokenizer.Tokenize("");
            Assert.Empty(res);
        }

        [Theory]
        [InlineData("a")]
        [InlineData("abcd ")]
        [InlineData("a123 ")]
        [InlineData(".a")]
        [InlineData("a.b.c.d")]
        [InlineData("_a")]
        [InlineData("a_b_c")]
        [InlineData("_")]
        [InlineData("[")]
        [InlineData("]")]
        [InlineData("{")]
        [InlineData("}")]
        [InlineData("<")]
        [InlineData(">")]
        [InlineData("&")]
        [InlineData("*")]
        [InlineData("+")]
        [InlineData("-")]
        [InlineData("!")]
        [InlineData("@")]
        [InlineData("#")]
        [InlineData("$")]
        [InlineData("%")]
        [InlineData("^")]
        [InlineData("=")]
        [InlineData(":")]
        [InlineData(";")]
        [InlineData("|")]
        [InlineData("\\")]
        [InlineData(".")]
        [InlineData(",")]
        [InlineData("`")]
        [InlineData("~")]
        public void TestSingleSymbol(string input)
        {
            var res = Tokenizer.Tokenize(input).ToList().Select(x => x.Kind);
            Assert.Single(res, SExprToken.Symbol);
        }

        [Theory]
        [InlineData(" \"\" ")]
        [InlineData("\"something\"")]
        [InlineData("\"another thing\"")]
        [InlineData("\"12346!@#$@#%&^$%&*\"")]
        [InlineData("\"\\n \\r something\"")]
        [InlineData("\"\\\\ all kinds of stuff\"    ")]
        [InlineData("\"     \"")]
        [InlineData("\"() -_ somehting\"")]
        public void TestSingleString(string input)
        {
            var res = Tokenizer.Tokenize(input).ToList().Select(x => x.Kind);
            Assert.Single(res, SExprToken.String);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("123")]
        [InlineData("123123245643465745675678960975")]
        [InlineData("-123")]
        [InlineData("0000000123000000")]
        [InlineData("-000123")]
        public void TestSingleInt(string input)
        {
            var res = Tokenizer.Tokenize(input).ToList().Select(x => x.Kind);
            Assert.Single(res, SExprToken.Integer);
        }

        [Theory]
        [InlineData("0.0")]
        [InlineData("123.0")]
        [InlineData("123123245.643465745675678960975")]
        [InlineData("-123.0")]
        [InlineData("000000012300.0000")]
        [InlineData("+000000012300.0000")]
        [InlineData("-00.0123")]
        [InlineData("1e10")]
        [InlineData("1e-10")]
        [InlineData("-1e-10")]
        [InlineData("-1e10")]
        [InlineData("-1e0")]
        [InlineData("1.34e10")]
        [InlineData("1.123e-10")]
        [InlineData("-1.098e-10")]
        [InlineData("-1.345e10")]
        [InlineData("-1.6747984000e0")]
        [InlineData("-1E10")]
        [InlineData("-1E0")]
        [InlineData("1.34E10")]
        [InlineData("1.123E-10")]
        public void TestSingleFloat(string input)
        {
            var res = Tokenizer.Tokenize(input).ToList().Select(x => x.Kind);
            Assert.Single(res, SExprToken.Float);
        }

        [Theory]
        [InlineData("(")]
        [InlineData("( ")]
        [InlineData("  (  ")]
        public void TestSingleOpen(string input)
        {
            var res = Tokenizer.Tokenize(input).ToList().Select(x => x.Kind);
            Assert.Single(res, SExprToken.Open);
        }

        [Theory]
        [InlineData(")")]
        [InlineData(") ")]
        [InlineData("  )  ")]
        public void TestSingleClose(string input)
        {
            var res = Tokenizer.Tokenize(input).ToList().Select(x => x.Kind);
            Assert.Single(res, SExprToken.Close);
        }

        [Theory]
        [InlineData("", new SExprToken[] { })]
        [InlineData("a", new SExprToken[] { SExprToken.Symbol })]
        [InlineData("(a \"bc\" 123)", new SExprToken[] { SExprToken.Open, SExprToken.Symbol, SExprToken.String, SExprToken.Integer, SExprToken.Close })]
        [InlineData("(()((", new SExprToken[] { SExprToken.Open, SExprToken.Open, SExprToken.Close, SExprToken.Open, SExprToken.Open })]
        [InlineData("(a \"bc\" 123.01)", new SExprToken[] { SExprToken.Open, SExprToken.Symbol, SExprToken.String, SExprToken.Float, SExprToken.Close })]
        [InlineData("(a \"bc\" 123 0.0)", new SExprToken[] { SExprToken.Open, SExprToken.Symbol, SExprToken.String, SExprToken.Integer, SExprToken.Float, SExprToken.Close })]
        [InlineData("(a [] somthing + - / *)", 
            new SExprToken[] { SExprToken.Open,
                SExprToken.Symbol, SExprToken.Symbol, SExprToken.Symbol, SExprToken.Symbol,
                SExprToken.Symbol, SExprToken.Symbol, SExprToken.Symbol,
            SExprToken.Close})]
        public void TestAllTogether(string input, SExprToken[] tokens)
        {
            var res = Tokenizer.Tokenize(input).ToList().Select(x => x.Kind);
            Assert.Equal(tokens.ToList(), res);
        }

    }
}
