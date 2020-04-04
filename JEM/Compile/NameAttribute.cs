using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Compile
{
    class NameAttribute : Attribute
    {
        public string Name { get; set; }

        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}
