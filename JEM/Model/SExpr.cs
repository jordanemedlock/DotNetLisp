﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Text;

namespace JEM.Model
{
    public class SExpr : Expr, IEnumerable<Expr>
    {
        public List<Expr> Values { get; set; }
        
        public SExpr(List<Expr> values)
        {
            Values = values;
        }


        public override string ToString(bool top = true) {
            var inners = String.Join(" ", Values.Select(v => v.ToString(false)));
            if (top) {
                return inners;
            } else {
                return "(" + inners + ")";
            }
        }

        public Expr Head()
        {
            return Values.FirstOrDefault();
        }

        public bool HeadIs(object head)
        {
            return Head().Equals(head);
        }

        public IEnumerator<Expr> GetEnumerator()
        {
            return ((IEnumerable<Expr>)Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Expr>)Values).GetEnumerator();
        }

        public Expr this[int index]
        {
            get
            {
                return Values[index];
            }
        }

        public long Count { get => Values.Count; }
    }
}
