﻿using System;
using System.Collections.Generic;
using System.IO;
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
            var s = new StringConstant("A", true);
            var res = Util.SQStringConstant.Value().Compile(s);
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
            var stringConstant = new StringConstant("a", true);
            var symbol = new Symbol("b");
            var compiler = Util.SQStringConstant.Value()
                .Or(Util.Symbol);
            var res = compiler.Compile(symbol);
            HasValue(res, symbol.Value);

            res = compiler.Compile(stringConstant);
            HasValue(res, stringConstant.Value);
        }
        
        [Theory]
        [JsonFileData(@"TestFiles\compiler_data.json")]
        public void TestCompilerData(string compilerName, Dictionary<string, object> pairs)
        {
            var field = typeof(ILFile).GetField(compilerName);
            var compiler = (Compiler<Expr, string>) field.GetValue(null);

            foreach (var kvp in pairs)
            {
                var parsed = SExprParser.Parse(kvp.Key);
                compiler.Generate();
                var name = compiler.Name;
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
                        Assert.False(res.HasValue, $"Result has value: {res.Value} in {compilerName}");
                    }
                } 
            }
        }


        [Theory]
        [InlineData(@"TestFiles\hello_world.jem", @"TestFiles\hello_world.il", @"TestFiles\hello_world.expected.il")]
        public void TestCompileFiles(string inputFile, string outputFile, string expectedFile) {
            Assert.True(File.Exists(inputFile));

            
            if (File.Exists(outputFile)) {
                File.Delete(outputFile);
            }


            Program.CompileFile(inputFile);
            Assert.True(File.Exists(outputFile));

            string outputContents = File.ReadAllText(outputFile);
            string expectedContents = File.ReadAllText(expectedFile);

            Assert.Equal(outputContents, expectedContents);

        }
    }
}
