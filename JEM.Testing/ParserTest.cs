using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JEM.Model;
using JEM.Parse;
using Superpower;
using Superpower.Model;
using Xunit;

namespace JEM.Testing
{
    public class ParserTest
    {

        [Fact]
        public void TestEmptyString()
        {
            var res = SExprParser.Parse("");
            Assert.IsType<SExpr>(res);
            Assert.Empty(res);
        }

        [Theory]
        // [InlineData("a", "a")]
        // [InlineData("abcd ", "abcd")]
        // [InlineData("a123 ", "a123")]
        // [InlineData(".a", ".a")]
        // [InlineData("a.b.c.d", "a.b.c.d")]
        // [InlineData("_a", "_a")]
        // [InlineData("a_b_c", "a_b_c")]
        // [InlineData("_", "_")]
        [InlineData("&", "&")]
        [InlineData("<*>", "<*>")]
        [InlineData("<<<", "<<<")]
        [InlineData("-+-", "-+-")]
        public void TestSingleSymbol(string input, string output)
        {
            var res = SExprParser.Parse(input);
            Assert.Single(res, output);
            Assert.IsType<Symbol>(res[0]);
            Assert.NotEqual(TextSpan.None, res[0].TextSpan);
        }

        [Theory]
        [InlineData(" \"\" ", "")]
        [InlineData("\"something\"", "something")]
        [InlineData("\"another thing\"", "another thing")]
        [InlineData("\"12346!@#$@#%&^$%&*\"", "12346!@#$@#%&^$%&*")]
        [InlineData("\"\\n \\r something\"", "\n \r something")]
        [InlineData("\"\\\\ all kinds of stuff\"    ", "\\ all kinds of stuff")]
        [InlineData("\"     \"", "     ")]
        [InlineData("\"() -_ somehting\"", "() -_ somehting")]
        public void TestSingleString(string input, string output)
        {
            var res = SExprParser.Parse(input);
            Console.WriteLine(res.ToString());
            Assert.IsType<JEM.Model.StringConstant>(res[0]);
            Assert.Single(res, output);
            Assert.NotEqual(TextSpan.None, res[0].TextSpan);
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("123", 123)]
        [InlineData("123123245643465675", 123123245643465675)]
        [InlineData("-123", -123)]
        [InlineData("0000000123000000", 123000000)]
        [InlineData("-000123", -123)]
        public void TestSingleInt(string input, long output)
        {
            var res = SExprParser.Parse(input);
            Console.WriteLine(res.ToString());
            Assert.IsType<JEM.Model.IntConstant>(res[0]);
            Assert.Single(res, output);
            Assert.NotEqual(TextSpan.None, res[0].TextSpan);
        }

        [Theory]
        [InlineData("0.0", 0.0)]
        [InlineData("123.0", 123.0)]
        [InlineData("123123245.643465745675678960975", 123123245.643465745675678960975)]
        [InlineData("-123.0", -123.0)]
        [InlineData("000000012300.0000", 12300.0)]
        [InlineData("-00.0123", -0.0123)]
        [InlineData("1.05e10", 1.05e10)]
        [InlineData("1e-10", 1e-10)]
        [InlineData("-1e-10", -1e-10)]
        [InlineData("-1e10", -1e10)]
        [InlineData("-1e0", -1e0)]
        public void TestSingleFloat(string input, double output)
        {
            var res = SExprParser.Parse(input);
            Console.WriteLine(res.ToString());
            Assert.IsType<JEM.Model.FloatConstant>(res[0]);
            Assert.Single(res, output);
            Assert.NotEqual(TextSpan.None, res[0].TextSpan);

        }

        [Theory]
        [InlineData("()")]
        [InlineData("( )")]
        [InlineData("  (  )   ")]
        public void TestEmptySExpr(string input)
        {
            var res = SExprParser.Parse(input);
            Assert.IsType<SExpr>(res);
            Assert.Single(res);
            Assert.IsType<SExpr>(res[0]);
            Assert.Empty(res.As<SExpr>()[0].As<SExpr>());
            Assert.NotEqual(TextSpan.None, res[0].TextSpan);

        }

        [Fact]

        public void TestAllTogether()
        {
            // have to do this in the test because attributes dont understand objects
            Dictionary<string, SExpr> cases = new Dictionary<string, SExpr>()
            {
                [""] = new SExpr(),
                ["a"] = new SExpr(new Symbol("a")),
                ["(a \"bc\" 123)"] = new SExpr(new SExpr(new Symbol("a"), new StringConstant("bc", false), new IntConstant(123))),
                ["(a \'bc\' 123)"] = new SExpr(new SExpr(new Symbol("a"), new StringConstant("bc", true), new IntConstant(123))),
                ["(* & [])"] = new SExpr(new SExpr(new Symbol("*"), new Symbol("&"), new Symbol("[]"))),
                ["(() () ())"] = new SExpr(new SExpr(new SExpr(), new SExpr(), new SExpr())),
                ["(( (a) ))"] = new SExpr(new SExpr(new SExpr(new SExpr(new Symbol("a"))))),
                ["1:10"] = new SExpr(new IntConstant(1), new Symbol(":"), new IntConstant(10)),
                ["(.line 1:10)"] = new SExpr(new SExpr(new Symbol(".line"), new IntConstant(1), new Symbol(":"), new IntConstant(10)))
            };

            foreach(var keyValuePair in cases) {
                var res = SExprParser.Parse(keyValuePair.Key);
                Assert.Equal(keyValuePair.Value, res);
                Assert.NotEqual(TextSpan.None, res.TextSpan);
            }
        }
    }
}
