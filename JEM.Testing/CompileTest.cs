using System;
using System.Collections.Generic;
using System.Text;
using JEM.Compile;
using JEM.Compile.CIL;
using JEM.Model;
using JEM.Parse;
using Xunit;

namespace JEM.Testing
{
    public class CompileTest
    {
        DottedName dottedNameCompiler = new DottedName();

        [Fact]
        public void TestSymbol()
        {
            var s = new Symbol("A");
            var res = Util.Symbol(s);
            Assert.Single(res);
            Assert.Equal(s.Value, res[0].Value);
        }

        [Fact]
        public void TestString()
        {
            var s = new StringConstant("A");
            var res = Util.StringConstant(s);
            Assert.Single(res);
            Assert.Equal(s.Value, res[0].Value);
        }

        [Fact]
        public void TestInt()
        {
            var s = new IntConstant(123);
            var res = Util.IntConstant(s);
            Assert.Single(res);
            Assert.Equal(s.Value, res[0].Value);
        }

        [Fact]
        public void TestFloat()
        {
            var f = new FloatConstant(123.34);
            var res = Util.FloatConstant(f);
            Assert.Single(res);
            Assert.Equal(f.Value, res[0].Value);
        }

        [Fact]
        public void TestBool()
        {
            var f = new BoolConstant(true);
            var res = Util.BoolConstant(f);
            Assert.Single(res);
            Assert.Equal(f.Value, res[0].Value);
        }

        [Fact]
        public void TestNull()
        {
            var f = new NullConstant();
            var res = Util.NullConstant(f);
            Assert.Single(res);
            Assert.Null(res[0].Value);
        }

        [Fact]
        public void TestOr()
        {
            var stringConstant = new StringConstant("a");
            var symbol = new Symbol("b");
            var compiler = Util.StringConstant
                .Or(Util.Symbol);
            var res = compiler(symbol);
            Assert.Single(res);
            Assert.Equal(symbol.Value, res[0].Value);

            res = compiler(stringConstant);
            Assert.Single(res);
            Assert.Equal(stringConstant.Value, res[0].Value);
        }

        [Theory]
        [InlineData(".hash algorithm 1", true)]
        [InlineData(".hash algorithm -123", true)]
        [InlineData(".hash something nothing", false)]
        public void TestHashAlgorithm(string input, bool success)
        {
            var parsed = SExprParser.Parse(input);
            var results = ILFile.HashAlg(parsed);
            if (success)
            {
                Assert.Single(results);
                Console.WriteLine(results[0].Value);
            }
            else
            {
                Assert.Empty(results);
            }

        }

        [Theory]
        [InlineData(".ver 1 0 0 0", ".ver 1 : 0 : 0 : 0")]
        public void TestVersion(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var results = ILFile.Version(parsed);
            Assert.Single(results);
            Assert.Equal(output, results[0].Value);

        }

        [Theory]
        [InlineData(".culture \"en-us\"", ".culture \"en-us\"")]
        public void TestCulture(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var results = ILFile.Culture(parsed);
            Assert.Single(results);
            Assert.Equal(output, results[0].Value);

        }

        [Theory]
        [InlineData(".culture \"en-us\"", ".culture \"en-us\"")]
        [InlineData(".ver 1 0 0 0", ".ver 1 : 0 : 0 : 0")]
        [InlineData(".hash algorithm 1", ".hash algorithm 1")]
        public void TestAsmDecl(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var results = ILFile.AsmDecl(parsed);
            Assert.Single(results);
            Assert.Equal(output, results[0].Value);

        }

        [Theory]
        [InlineData(".assembly extern (.culture \"en-us\")", ".assembly extern {\n\t.culture \"en-us\"\n}")]
        [InlineData(".assembly extern (.ver 1 0 0 0)", ".assembly extern {\n\t.ver 1 : 0 : 0 : 0\n}")]
        [InlineData(".assembly extern (.hash algorithm 1)", ".assembly extern {\n\t.hash algorithm 1\n}")]
        public void TestExternAssembly(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var results = ILFile.ExternAssembly(parsed);
            Assert.Single(results);
            Assert.Equal(output, results[0].Value);

        }

        //[Theory]
        //[InlineData("a")]
        //[InlineData("a.b.c")]
        //[InlineData("a.asdfjlksadf")]
        //[InlineData("a_ASDFN.sdlkf")]
        //// TODO: Add negative cases
        //public void TestDottedNameCompiler(string input)
        //{
        //    var expr = new Symbol(input);
        //    Assert.True(dottedNameCompiler.MatchesPattern(expr));
        //    var str = dottedNameCompiler.Transform(expr);
        //    Assert.Equal(str, input);
        //}

        //AsmDecl asmDeclCompiler = new AsmDecl();

        //[Theory]
        //[InlineData(".hash algorithm 123", null)]
        //public void TestAsmDecl(string input, string output)
        //{
        //    var expr = SExprParser.Parse(input);
        //    Assert.True(asmDeclCompiler.MatchesPattern(expr));
        //    var str = asmDeclCompiler.Transform(expr);
        //    Assert.Equal(output ?? input, str);
        //}
    }
}
