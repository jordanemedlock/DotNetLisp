using System;
using System.Collections.Generic;
using System.Text;

namespace JEM
{
    public class Symbol : Value
    {
        public string Value { get; set; }

        public string ToString(bool top = true)
        {
            return Value;
        }

        public Symbol(string value)
        {
            Value = value;
        }
    }
}
