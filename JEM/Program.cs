using System;
using System.Collections;
using System.Collections.Generic;
using JEM.Model;
using JEM.Parse;
using Superpower;

namespace JEM
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = "abc123 () (something (a)) esle";
            var tokenList = new SExprTokenizer().TryTokenize(str);
            var x = SExprParser.Values.Parse(tokenList.Value);
            Console.WriteLine(new SExpr(x).ToString());
        }

    }
}
