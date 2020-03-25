using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JEM.Model;

namespace JEM.Compile.CIL
{
    class Assembly
    {
        //public bool MatchesPattern(Expr input)
        //{
        //    return input.As<SExpr>()?.HeadIs(".assembly") ?? false;
        //}

        //public string Transform(Expr input)
        //{
        //    var sexpr = input.As<SExpr>();
        //    var @extern = sexpr.Count == 3 && sexpr[1].Equals("extern") ? "extern" : "";
        //    var dottedName = new DottedName().Transform(sexpr[2]);
        
        //    var asmDecls = sexpr[3].As<SExpr>().Select(x => new AsmDecl().Transform(x));
        //    return $".assembly {@extern} {dottedName} {{\n{String.Join('\n',asmDecls)}\n}}";
        //}
    }
}
