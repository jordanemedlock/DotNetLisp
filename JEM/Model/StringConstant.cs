using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
    public class StringConstant : Expr
    {
        public string Value { get; set; }

        public StringConstant(string value)
        {
            Value = value;
        }

        public override string ToString(bool top = false) {
          return "\""+Value+"\"";
        }

        public override bool Equals(object other)
        {
            if (other is String str)
            {
                return Value.Equals(str);
            } 
            else if (other is StringConstant strC)
            {
                return Value.Equals(strC.Value);
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
