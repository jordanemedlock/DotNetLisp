using JEM.Model;
using JEM.Compile;

namespace JEM.PreCompile
{
    public class PreCompiler
    {
        public static Compiler<Expr, Expr> PreCompile = new Compiler<Expr, Expr>("PreCompile", input =>
            new CompilerResult<Expr, Expr>(input, null)
        );

        public static CompilerResult<Expr, Expr> Compile(Expr input) 
        {
            return PreCompile.Compile(input);
        }
    }
    
}