using System;
using System.Collections.Generic;
using System.Text;

namespace JEM
{
    struct StringValue : Value
    {
        public string Value { get; set; }

        public StringValue(string value)
        {
            Value = value;
        }

        public string ToString(bool top = false) {
          return Value;
        }

    }
}
