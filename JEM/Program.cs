using System;
using System.Collections;
using System.Collections.Generic;
using JEM.Compile.CIL;
using JEM.Model;
using JEM.Parse;
using JEM.PreCompile;
using Superpower;
using System.IO;
using Superpower.Model;

namespace JEM
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) {
                return;
            }
            var filePath = args[0];
            CompileFile(filePath);
        }

        public static void CompileFile(string filePath) {
            var contents = ReadFile(filePath);
            var tokens = TokenizeString(contents);
            var exprs = ParseTokenList(tokens);
            var preCompiled = PreCompileExpr(exprs);
            var compiled = CompileToIL(preCompiled);
            var outputFilePath = Path.GetFileNameWithoutExtension(filePath) + ".il";
            SaveToFile(outputFilePath, compiled);
        }

        public static string ReadFile(string filePath) 
        {
            try 
            {
                string contents = File.ReadAllText(filePath);
                return contents;
            }
            catch (IOException e)
            {
                Console.WriteLine("File read failed with exception: " + e.Message);
            }
            return "";
        }

        public static TokenList<SExprToken> TokenizeString(string fileContents) 
        {
            var result = SExprTokenizer.Instance.TryTokenize(fileContents);

            if (result.HasValue) 
            {
                return result.Value;
            }
            else 
            {
                Console.WriteLine("Tokenizing failed with error: " + result.ErrorMessage);
            }
            return new TokenList<SExprToken>();
        }

        public static SExpr ParseTokenList(TokenList<SExprToken> tokenList) 
        {
            var result = SExprParser.Exprs.TryParse(tokenList);

            if (result.HasValue) 
            {
                return new SExpr(result.Value);
            }
            else 
            {
                Console.WriteLine("Parser failed with error: " + result.ErrorMessage);
            }
            return null;
        }

        public static Expr PreCompileExpr(Expr input) 
        {
            var result = PreCompiler.Compile(input);

            if (result.HasValue) 
            {
                return result.Value;
            }
            else 
            {
                Console.WriteLine("Pre Compiler failed with error: " + result.Error);
            }
            return null;
        }

        public static string CompileToIL(Expr ilFile) 
        {
            var result = ILFile.Compiler.Compile(ilFile);

            if (result.HasValue)  
            {
                return result.Value;
            }
            else
            {
                Console.WriteLine("IL Compiler failed with error: " + result.Error);
            }
            return "";
        }

        public static void SaveToFile(string filePath, string fileContents)
        {
            File.WriteAllText(filePath, fileContents);
        }
    }
}
