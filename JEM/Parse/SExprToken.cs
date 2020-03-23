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

        [Token(Example = "(")]
        Open,

        [Token(Example = ")")]
        Close
    }
}
