using System;
using System.Collections;
using System.Collections.Generic;

namespace JEM
{
    class Program
    {
        static void Main(string[] args)
        {
            var sexpr = SExprParser.ParseSExpr("a b c (d)");
            Console.WriteLine(sexpr.ToString());
            Console.ReadLine();
        }

    }
}
