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
        public override string ToString(bool top = false)
        {
            return $"{Value}";
        }
        public override bool Equals(object other)
        {
            if (other is long i)
            {
                return i == Value;
            }
            else if (other is IntConstant intC)
            {
                return intC.Value == Value;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Value);
        }
    }
}
