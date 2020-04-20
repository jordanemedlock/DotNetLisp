using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Model
{
  public class StringConstant : Expr
  {
    public string Value { get; set; }

    public StringConstant(string value)
    {
      Value = value;
    }

    public static StringConstant FromUnescaped(string value)
    {
      return new StringConstant(UnescapeString(value));
    }

    public override string ToString()
    {
      return Escaped();
    }

    public string Escaped()
    {
      return EscapeString(Value);
    }

    public override bool Equals(object other)
    {
      if (other is String str)
      {
        return Value.Equals(str);
      }
      else if (other is StringConstant strC)
      {
        return Value.Equals(strC.Value);
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
      return "\"" + ret + "\"";
    }
    public static string UnescapeString(string input)
    {
      var ret = input;
      if (input[0] == '"') input = input.Remove(0, 1);
      if (input[input.Length - 1] == '"') input = input.Remove(input.Length - 1, 1);
      foreach (var kvp in escapeMapping)
      {
        ret = ret.Replace(kvp.Value, kvp.Key);
      }
      return ret;
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
