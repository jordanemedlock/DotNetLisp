using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;

namespace JEM.Model
{
    public class SExpr : Expr
    {
        public List<Expr> Values { get; set; }
        
        public SExpr(List<Expr> values)
        {
            Values = values;
        }


        public string ToString(bool top = true) {
            var inners = String.Join(" ", Values.Select(v => v.ToString(false)));
            if (top) {
                return "<" + inners + ">";
            } else {
                return "(" + inners + ")";
            }
        }

        public static SExpr Parse(string input) {
            return Parse(Tokenize(input), true).Item1;
        }

        private static Tuple<SExpr, List<string>> Parse(List<string> tokens, bool top)
        {

            var values = new List<Expr>();
            var right = tokens;

            while (right.Count > 0) 
            {
                var token = right[0];
                right.RemoveAt(0);
                switch (token) {
                case "(":
                    var pair = Parse(right, false);
                    var sexpr = pair.Item1;
                    right = pair.Item2;
                    values.Add(sexpr);
                    break;
                case ")":
                    if (!top) {
                        return new Tuple<SExpr, List<string>>(new SExpr(values), right);
                    } else {
                        throw new Exception("unmatched parentheses");
                    }
                default:
                    values.Add(new StringConstant(token));
                    break;
                }
            }
            if (top) {
                return new Tuple<SExpr, List<string>>(new SExpr(values), right);
            } else {
                throw new Exception("unmatched parentheses");
            }
        }
        private static List<string> Tokenize(string input)
        {
            string token = "";
            List<string> ret = new List<string>();
            foreach (char c in input)
            {
                switch (c)
                {
                    case '(':
                    case ')':
                        if (!string.IsNullOrEmpty(token))
                        {
                            ret.Add(token);
                            token = "";
                        }
                        ret.Add(c + "");
                        break;
                    case ' ':
                        if (!string.IsNullOrEmpty(token))
                        {
                            ret.Add(token);
                            token = "";
                        }
                        break;
                    default:
                        token += c;
                        break;
                }
            }
            if (!string.IsNullOrEmpty(token)) {
                ret.Add(token);
            }
            return ret;
        }
    }
}
