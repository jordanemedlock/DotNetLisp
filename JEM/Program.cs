using System;
using System.Collections;
using System.Collections.Generic;

namespace JEM
{
    class Program
    {
        static void Main(string[] args)
        {
            var sexpr = SExpr.Parse("a b (c d (e) () )");
            Console.WriteLine(sexpr.ToString());
        }

    }
}
