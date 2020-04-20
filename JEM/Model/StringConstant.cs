using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
  public class StringConstant : Expr
  {
    public string Value { get; set; }

    public bool SingleQuote { get; set; }

    public string Quote => SingleQuote ? "'" : "\"";

    public StringConstant(string value, bool singleQuote)
    {
      Value = value;
      SingleQuote = singleQuote;
    }

    public static StringConstant FromUnescaped(string value)
    {
      return new StringConstant(UnescapeString(value), IsSingleQuote(value));
    }

    public override string ToString()
    {
      return Quote + Escaped() + Quote;
    }

    public string Escaped()
    {
      return Quote + EscapeString(Value) + Quote;
    }

    public override bool Equals(object other)
    {
      if (other is String str)
      {
        return Value.Equals(str);
      }
      else if (other is StringConstant strC)
      {
        return Value.Equals(strC.Value) && SingleQuote == strC.SingleQuote;
      }
      else
      {
        return false;
      }
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(base.GetHashCode(), Value);
    }


    public static string EscapeString(string input)
    {
      var ret = input;
      foreach (var kvp in escapeMapping)
      {
        ret = ret.Replace(kvp.Key, kvp.Value);
      }
      return ret;
    }
    public static string UnescapeString(string input)
    {
      var ret = input;
      if (input[0] == '"' || input[0] == '\'') input = input.Remove(0, 1);
      if (input[input.Length - 1] == '"' || input[input.Length - 1] == '\'') input = input.Remove(input.Length - 1, 1);
      foreach (var kvp in escapeMapping)
      {
        ret = ret.Replace(kvp.Value, kvp.Key);
      }
      return ret;
    }

    public static bool IsSingleQuote(string input) 
    {
      return input[0] == '\'';
    }

    private static Dictionary<string, string> escapeMapping = new Dictionary<string, string>()
        {
            {"\"", @"\\\"""},
            {"\\\\", @"\\"},
            {"\a", @"\a"},
            {"\b", @"\b"},
            {"\f", @"\f"},
            {"\n", @"\n"},
            {"\r", @"\r"},
            {"\t", @"\t"},
            {"\v", @"\v"},
            {"\0", @"\0"},
        };
  }
}
