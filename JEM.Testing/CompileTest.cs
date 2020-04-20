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
            var res = Util.Symbol.Compile(s);
            HasValue(res, s.Value);
        }

        [Fact]
        public void TestString()
        {
            var s = new StringConstant("A");
            var res = Util.StringConstant.Select(x => x.Value).Compile(s);
            HasValue(res, s.Value);
        }

        [Fact]
        public void TestInt()
        {
            var s = new IntConstant(123);
            var res = Util.IntConstant.Compile(s);
            HasValue(res, s.Value);
        }

        [Fact]
        public void TestFloat()
        {
            var f = new FloatConstant(123.34);
            var res = Util.FloatConstant.Compile(f);
            HasValue(res, f.Value);
        }

        [Fact]
        public void TestBool()
        {
            var f = new BoolConstant(true);
            var res = Util.BoolConstant.Compile(f);
            HasValue(res, f.Value);
        }

        [Fact]
        public void TestNull()
        {
            var f = new NullConstant();
            var res = Util.NullConstant.Compile(f);
            HasValue(res, null);
        }

        [Fact]
        public void TestOr()
        {
            var stringConstant = new StringConstant("a");
            var symbol = new Symbol("b");
            var compiler = Util.StringConstant.Select(x => x.Value)
                .Or(Util.Symbol);
            var res = compiler.Compile(symbol);
            HasValue(res, symbol.Value);

            res = compiler.Compile(stringConstant);
            HasValue(res, stringConstant.Value);
        }
        
        [Theory]
        [JsonFileData("compiler_data.json")]
        public void TestCompilerData(string compilerName, Dictionary<string, object> pairs)
        {
            var compiler = (Compiler<Expr, string>) typeof(ILFile).GetField(compilerName).GetValue(null);

            foreach (var kvp in pairs)
            {
                var parsed = SExprParser.Parse(kvp.Key);
                var res = compiler.Compile(parsed[0]);
                if (kvp.Value is string s)
                {
                    Assert.True(res.HasValue, $"Result from \"{kvp.Key}\" in {compilerName} has no value with error: {res.Error}");
                    Assert.Equal(s, res.Value);
                }
                else if (kvp.Value is bool b)
                {
                    if (b)
                    {
                        Assert.True(res.HasValue, $"Result has no value with error: {res.Error}");
                        Assert.Equal(kvp.Key, res.Value);
                    }
                    else
                    {
                        Assert.False(res.HasValue, $"Result has value: {res.Value}");
                    }
                } 
            }
        }

    }
}
