
namespace JEM.Model
{
    public class Operator : Expr
    {

        public string Value { get; set; }

        public Operator(string value)
        {
            Value = value;
        }


        public override bool Equals(object other)
        {
            if (other is Operator @operator)
            {
                return @operator.Value.Equals(this.Value);
            }
            else if (other is string str)
            {
                return Value.Equals(str);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }
    }
}