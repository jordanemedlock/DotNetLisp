using System;
using System.Collections.Generic;
using System.Text;
using Superpower.Display;

namespace JEM.Parse
{
    public enum SExprToken
    {
        None,

        Symbol,
        Integer,
        String,
        Float,


        [Token(Example = "(")]
        Open,

        [Token(Example = ")")]
        Close
    }
}
