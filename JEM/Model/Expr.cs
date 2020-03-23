using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
    public interface Expr
    {
      string ToString(bool top = false);
    }
}
