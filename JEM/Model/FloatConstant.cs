using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
    class FloatConstant : Expr
    {
        public double Value { get; set; }

        public FloatConstant(double value)
        {
            Value = value;
        }

        public string ToString(bool top)
        {
            return $"{Value}";
        }
    }
}
