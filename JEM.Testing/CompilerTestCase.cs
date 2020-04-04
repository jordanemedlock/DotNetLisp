using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Testing
{
    [Serializable]
    public class CompilerTestCase
    {
        public Dictionary<string, string> IOPairs { get; set; }
        public string Compiler { get; set; }
    }
}
