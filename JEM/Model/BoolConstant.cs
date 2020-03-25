using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
    public class BoolConstant : Expr
    {
        public bool Value { get; set; }

        public BoolConstant(bool value)
        {
            Value = value;
        }

        public string ToString(bool top)
        {
            return Value ? "true" : "false";
        }
    }
}
