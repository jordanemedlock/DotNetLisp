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

        public static Compiler<Expr, string> HashAlg =
            Util.Next(Util.SymbolIs(".hash")).Bind(_1 => 
            Util.Next(Util.SymbolIs("algorithm")).Bind(_2 => 
            Util.Next(Util.IntConstant).Bind(i =>
            Compile.Return<Expr, string>($".hash algorithm {i}"))));
    }
}
