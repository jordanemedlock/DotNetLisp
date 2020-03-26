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
        [InlineData("A.B.C")]
        [InlineData("A")]
        [InlineData("A.123")]
        [InlineData(".A.B.C", Skip = "Should fail in the future")]
        public void TestDottedName(string input)
        {
            var parsed = new Symbol(input);
            var results = ILFile.DottedName(parsed);
            Assert.Single(results);
            Assert.Equal(input, results[0].Value);
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
        [InlineData(".assembly extern A.B.C ((.culture \"en-us\"))", ".assembly extern A.B.C {\n\t.culture \"en-us\"\n}")]
        [InlineData(".assembly extern A ((.ver 1 0 0 0))", ".assembly extern A {\n\t.ver 1 : 0 : 0 : 0\n}")]
        [InlineData(".assembly extern F ((.hash algorithm 1))", ".assembly extern F {\n\t.hash algorithm 1\n}")]
        [InlineData(".assembly extern .something ((.hash algorithm 1) (.ver 1 0 0 0))", ".assembly extern .something {\n\t.hash algorithm 1\n.ver 1 : 0 : 0 : 0\n}", Skip = "Should fail in the future")] // this is going to fail in the future
        [InlineData(".assembly extern anything ((.hash algorithm 1) (.ver 1 0 0 0) (.culture \"en-us\"))", ".assembly extern anything {\n\t.hash algorithm 1\n.ver 1 : 0 : 0 : 0\n.culture \"en-us\"\n}")]
        public void TestExternAssembly(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var results = ILFile.ExternAssembly(parsed);
            Assert.Single(results);
            Assert.Equal(output, results[0].Value);
        }

        [Theory]
        [InlineData(".assembly A.B.C ((.culture \"en-us\"))", ".assembly A.B.C {\n\t.culture \"en-us\"\n}")]
        [InlineData(".assembly A ((.ver 1 0 0 0))", ".assembly A {\n\t.ver 1 : 0 : 0 : 0\n}")]
        [InlineData(".assembly F ((.hash algorithm 1))", ".assembly F {\n\t.hash algorithm 1\n}")]
        [InlineData(".assembly .something ((.hash algorithm 1) (.ver 1 0 0 0))", ".assembly .something {\n\t.hash algorithm 1\n.ver 1 : 0 : 0 : 0\n}", Skip = "Should fail in the future")] // this is going to fail in the future
        [InlineData(".assembly anything ((.hash algorithm 1) (.ver 1 0 0 0) (.culture \"en-us\"))", ".assembly anything {\n\t.hash algorithm 1\n.ver 1 : 0 : 0 : 0\n.culture \"en-us\"\n}")]
        public void TestNonExternAssembly(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var results = ILFile.NonExternAssembly(parsed);
            Assert.Single(results);
            Assert.Equal(output, results[0].Value);
        }


        [Theory]
        [InlineData(".assembly extern A.B.C ((.culture \"en-us\"))", ".assembly extern A.B.C {\n\t.culture \"en-us\"\n}")]
        [InlineData(".assembly extern A ((.ver 1 0 0 0))", ".assembly extern A {\n\t.ver 1 : 0 : 0 : 0\n}")]
        [InlineData(".assembly extern F ((.hash algorithm 1))", ".assembly extern F {\n\t.hash algorithm 1\n}")]
        [InlineData(".assembly extern .something ((.hash algorithm 1) (.ver 1 0 0 0))", ".assembly extern .something {\n\t.hash algorithm 1\n.ver 1 : 0 : 0 : 0\n}", Skip = "Should fail in the future")] // this is going to fail in the future
        [InlineData(".assembly extern anything ((.hash algorithm 1) (.ver 1 0 0 0) (.culture \"en-us\"))", ".assembly extern anything {\n\t.hash algorithm 1\n.ver 1 : 0 : 0 : 0\n.culture \"en-us\"\n}")]
        [InlineData(".assembly A.B.C ((.culture \"en-us\"))", ".assembly A.B.C {\n\t.culture \"en-us\"\n}")]
        [InlineData(".assembly A ((.ver 1 0 0 0))", ".assembly A {\n\t.ver 1 : 0 : 0 : 0\n}")]
        [InlineData(".assembly F ((.hash algorithm 1))", ".assembly F {\n\t.hash algorithm 1\n}")]
        [InlineData(".assembly .something ((.hash algorithm 1) (.ver 1 0 0 0))", ".assembly .something {\n\t.hash algorithm 1\n.ver 1 : 0 : 0 : 0\n}", Skip = "Should fail in the future")] // this is going to fail in the future
        [InlineData(".assembly anything ((.hash algorithm 1) (.ver 1 0 0 0) (.culture \"en-us\"))", ".assembly anything {\n\t.hash algorithm 1\n.ver 1 : 0 : 0 : 0\n.culture \"en-us\"\n}")]

        public void TestAssembly(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var results = ILFile.Assembly(parsed);
            Assert.Single(results);
            Assert.Equal(output, results[0].Value);
        }

        [Theory]
        [InlineData(".file nometadata \"some_filename.txt\" .hash (1 2 3 4) .entrypoint", ".file nometadata \"some_filename.txt\" .hash = (1 2 3 4) .entrypoint")]
        [InlineData(".file \"some_filename.txt\" .hash (1 2 3 4) .entrypoint", ".file  \"some_filename.txt\" .hash = (1 2 3 4) .entrypoint")]
        [InlineData(".file \"some_filename.txt\" .hash (1 2 3 4)", ".file  \"some_filename.txt\" .hash = (1 2 3 4) ")]

        public void TestFileDirective(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var results = ILFile.FileDirective(parsed);
            Assert.Single(results);
            Assert.Equal(output, results[0].Value);
        }

        [Theory]
        [InlineData(".field", ".field")]

        public void TestFieldDirective(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var results = ILFile.Field(parsed);
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
