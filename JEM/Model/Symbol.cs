using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
    public class Symbol : Expr
    {
        public string Value { get; set; }

        public override string ToString(bool top = true)
        {
            return Value;
        }

        public Symbol(string value)
        {
            Value = value;
        }

        public override bool Equals(object other)
        {
            if (other is Symbol symbol)
            {
                return symbol.Value.Equals(this.Value);
            }
            else if (other is string str)
            {
                return Value.Equals(str);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
}
