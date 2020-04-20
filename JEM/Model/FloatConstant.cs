using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
    public class FloatConstant : Expr
    {
        public double Value { get; set; }

        public FloatConstant(double value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{Value}";
        }

        public override bool Equals(object obj)
        {
            switch(obj)
            {
                case double d:
                    return d == Value;
                case FloatConstant f:
                    return f.Value == Value;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Value);
        }
    }
}
