using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
    class StringConstant : Expr
    {
        public string Value { get; set; }

        public StringConstant(string value)
        {
            Value = value;
        }

        public string ToString(bool top = false) {
          return "\""+Value+"\"";
        }

    }
}
