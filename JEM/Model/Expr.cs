using System;
using System.Collections.Generic;
using System.Text;
using Superpower.Model;

namespace JEM.Model
{
    public abstract class Expr
    {

        public TextSpan TextSpan { get; set; } = TextSpan.None;

        public virtual string ToString(bool top = false)
        {
            return "";
        }
        
        public override bool Equals(object other)
        {
            return base.Equals(other);
        }

        public TChild As<TChild>() where TChild : Expr
        {
            return this.Is<TChild>() ? (TChild)this : null;
        }

        public bool Is<TChild>() where TChild : Expr
        {
            return typeof(TChild).IsInstanceOfType(this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
