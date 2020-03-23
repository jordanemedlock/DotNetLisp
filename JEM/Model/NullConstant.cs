using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
    class NullConstant : Expr
    {
        public string ToString(bool top)
        {
            return "null";
        }
    }
}
