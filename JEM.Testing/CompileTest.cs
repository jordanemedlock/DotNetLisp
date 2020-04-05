using System;
using System.Collections.Generic;
using System.Linq;
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

        private void HasValue<T, U>(CompilerResult<T, U> res, U value)
        {
            Assert.True(res.HasValue, $"Result has no value with error: {res.Error}");
            Assert.Equal(value, res.Value);
        }

        [Fact]
        public void TestSymbol()
        {
            var s = new Symbol("A");
            var res = Util.Symbol(s);
            HasValue(res, s.Value);
        }

        [Fact]
        public void TestString()
        {
            var s = new StringConstant("A");
            var res = Util.StringConstant(s);
            HasValue(res, s.Value);
        }

        [Fact]
        public void TestInt()
        {
            var s = new IntConstant(123);
            var res = Util.IntConstant(s);
            HasValue(res, s.Value);
        }

        [Fact]
        public void TestFloat()
        {
            var f = new FloatConstant(123.34);
            var res = Util.FloatConstant(f);
            HasValue(res, f.Value);
        }

        [Fact]
        public void TestBool()
        {
            var f = new BoolConstant(true);
            var res = Util.BoolConstant(f);
            HasValue(res, f.Value);
        }

        [Fact]
        public void TestNull()
        {
            var f = new NullConstant();
            var res = Util.NullConstant(f);
            HasValue(res, null);
        }

        [Fact]
        public void TestOr()
        {
            var stringConstant = new StringConstant("a");
            var symbol = new Symbol("b");
            var compiler = Util.StringConstant
                .Or(Util.Symbol);
            var res = compiler(symbol);
            HasValue(res, symbol.Value);

            res = compiler(stringConstant);
            HasValue(res, stringConstant.Value);
        }

        

        [Theory]
        [InlineData("A.B.C")]
        [InlineData("A")]
        [InlineData("A.123")]
        [InlineData(".A.B.C", Skip = "Should fail in the future")]
        public void TestDottedName(string input)
        {
            var parsed = new Symbol(input);
            var res = ILFile.DottedName(parsed);
            HasValue(res, input);
        }

        [Theory]
        [InlineData(".hash algorithm 1", ".hash algorithm 1")]
        [InlineData(".hash algorithm -123", ".hash algorithm -123")]
        [InlineData(".hash something nothing", null)]
        public void TestHashAlgorithm(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var res = ILFile.HashAlg(parsed);
            if (output != null)
            {
                HasValue(res, output);
            }
            else
            {
                Assert.False(res.HasValue);
            }

        }

        [Theory]
        [InlineData(".ver 1 0 0 0", ".ver 1 : 0 : 0 : 0")]
        public void TestVersion(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var res = ILFile.Version(parsed);
            HasValue(res, output);

        }

        [Theory]
        [InlineData(".culture \"en-us\"", ".culture \"en-us\"")]
        public void TestCulture(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var res = ILFile.Culture(parsed);
            HasValue(res, output);

        }

        [Theory]
        [InlineData(".culture \"en-us\"", ".culture \"en-us\"")]
        [InlineData(".ver 1 0 0 0", ".ver 1 : 0 : 0 : 0")]
        [InlineData(".hash algorithm 1", ".hash algorithm 1")]
        public void TestAsmDecl(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var res = ILFile.AsmDecl(parsed);
            HasValue(res, output);

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
            var res = ILFile.ExternAssembly(parsed);
            HasValue(res, output);
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
            var res = ILFile.NonExternAssembly(parsed);
            HasValue(res, output);
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
            var res = ILFile.Assembly(parsed);
            HasValue(res, output);
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
        [InlineData(".corflags 123", ".corflags 123")]
        [InlineData(".file nometadata \"some_filename.txt\" .hash (1 2 3 4) .entrypoint", ".file nometadata \"some_filename.txt\" .hash = (1 2 3 4) .entrypoint")]
        [InlineData(".file \"some_filename.txt\" .hash (1 2 3 4) .entrypoint", ".file  \"some_filename.txt\" .hash = (1 2 3 4) .entrypoint")]
        [InlineData(".file \"some_filename.txt\" .hash (1 2 3 4)", ".file  \"some_filename.txt\" .hash = (1 2 3 4) ")]

        public void TestDecl(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var res = ILFile.Decl(parsed);

            HasValue(res, output);
        }

        [Theory]
        [InlineData(".corflags 123", ".corflags 123")]
        public void TestCorflags(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var res = ILFile.Corflags(parsed);
            HasValue(res, output);
        }

        [Theory]
        [InlineData(".file nometadata \"some_filename.txt\" .hash (1 2 3 4) .entrypoint", ".file nometadata \"some_filename.txt\" .hash = (1 2 3 4) .entrypoint")]
        [InlineData(".file \"some_filename.txt\" .hash (1 2 3 4) .entrypoint", ".file  \"some_filename.txt\" .hash = (1 2 3 4) .entrypoint")]
        [InlineData(".file \"some_filename.txt\" .hash (1 2 3 4)", ".file  \"some_filename.txt\" .hash = (1 2 3 4) ")]

        public void TestFileDirective(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var res = ILFile.FileDirective(parsed);
            HasValue(res, output);
        }

            
        
        [Theory()]
        [InlineData(".field private int32 xOrigin", ".field private int32 xOrigin", Skip = "Fails")]
        //[InlineData(".field public static initonly int32 pointCount", ".field public static initonly int32 pointCount")]
        //[InlineData(".field (Counter counter)", ".field Counter counter", Skip = "Failing, not sure if its actually valid")]
        public void TestFieldDirective(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var res = ILFile.Field(parsed);
            HasValue(res, output);
        }

        [Theory]
        [InlineData("private int32 xOrigin", "private int32 xOrigin", Skip = "Fails I need to make a many that procedes after it fails")]
        public void TestFieldDecl(string input, string output)
        {
            var parsed = SExprParser.Parse(input);
            var res = ILFile.FieldDecl(parsed);
            HasValue(res, output);
        }
        
        
        

        [Theory]
        [JsonFileData("compiler_data.json")]
        public void TestCompilerData(string compilerName, Dictionary<string, string> pairs)
        {
            var compiler = (Compiler<Expr, string>) typeof(ILFile).GetField(compilerName).GetValue(null);

            foreach (var kvp in pairs)
            {
                var parsed = SExprParser.Parse(kvp.Key);
                var res = compiler(parsed[0]);
                Assert.True(res.HasValue, $"Result has no value with error: {res.Error}");
                Assert.Equal(kvp.Value, res.Value);
            }
        }

    }
}
