using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
    public class IntConstant : Expr
    {
        public long Value { get; set; }

        public IntConstant(long value)
        {
            Value = value;
        }
        public string ToString(bool top = false)
        {
            return $"{Value}";
        }
    }
}
