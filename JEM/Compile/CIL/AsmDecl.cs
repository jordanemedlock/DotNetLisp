﻿using System;
using JEM.Model;

namespace JEM.Compile.CIL
{
    public class AsmDecl : ITransformer<Expr, string>
    {
        
        public AsmDecl()
        {
        }

        public bool MatchesPattern(Expr input)
        {
            return true;
        }

        public string Transform(Expr x)
        {
            return x.ToString();
        }
        
    }
}