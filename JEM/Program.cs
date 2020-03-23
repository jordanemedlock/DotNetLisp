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
            var str = "abc123 () (\"something\" (11.234)) 567";
            var x = SExprParser.Parse(str).ToString();
        }

    }
}
