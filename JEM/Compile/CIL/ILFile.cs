using System;
using System.Collections.Generic;
using System.Text;
using JEM.Model;
using JEM.Compile;


namespace JEM.Compile.CIL
{
    public static class ILFile
    {
        //public static Compiler<Expr, string> Compiler = Decl.Many();

        //public static Compiler<Expr, string> Decl = Assembly; // TODO: Or() a bunc of other shit

        //public static Compiler<Expr, string> Assembly = Compile.Id<Expr>().Select(x => x.ToString()); // TODO: fix this
        
        public static Compiler<Expr, string> DottedName = Util.Symbol;

        // 138
        public static Compiler<Expr, string> HashAlg =
            Util.Next(Util.SymbolIs(".hash")).Apply(
            Util.Next(Util.SymbolIs("algorithm")).Apply(
            Util.Next(Util.IntConstant).Bind(i =>
            Compile.Return<Expr, string>($".hash algorithm {i}"))));

        // 138
        public static Compiler<Expr, string> Version =
            Util.Next(Util.SymbolIs(".ver")).Apply(
            Util.Next(Util.IntConstant).Bind(major =>
            Util.Next(Util.IntConstant).Bind(minor =>
            Util.Next(Util.IntConstant).Bind(build =>
            Util.Next(Util.IntConstant).Bind(revision =>
            Compile.Return<Expr, string>($".ver {major} : {minor} : {build} : {revision}")
            )))));

        // 138
        public static Compiler<Expr, string> Culture =
            Util.Next(Util.SymbolIs(".culture")).Apply(
            Util.Next(Util.StringConstant).Bind(str => 
            Compile.Return<Expr, string>($".culture \"{EscapeString(str)}\"")
            ));

        // TODO: public key: Page 138 in ECMA-355

        // TODO: custom: Page 225

        // 138
        public static Compiler<Expr, string> AsmDecl =
            HashAlg
            .Or(Version)
            .Or(Culture);

        // 137
        public static Compiler<Expr, string> ExternAssembly =
            Util.Next(Util.SymbolIs(".assembly")).Apply(
            Util.Next(Util.SymbolIs("extern")).Apply(
            Util.Next(AsmDecl).Bind(asmDecl => // TODO: Many
            Compile.Return<Expr, string>($".assembly extern {{\n\t{asmDecl}\n}}")
            )));

        public static string EscapeString(string input)
        {
            var ret = input;
            foreach (var kvp in escapeMapping)
            {
                ret = ret.Replace(kvp.Key, kvp.Value);
            }
            return ret;
        }

        private static Dictionary<string, string> escapeMapping = new Dictionary<string, string>()
        {
            {"\"", @"\\\"""},
            {"\\\\", @"\\"},
            {"\a", @"\a"},
            {"\b", @"\b"},
            {"\f", @"\f"},
            {"\n", @"\n"},
            {"\r", @"\r"},
            {"\t", @"\t"},
            {"\v", @"\v"},
            {"\0", @"\0"},
        };
    }
}
