using System;
using System.Collections.Generic;
using System.Text;
using Superpower.Display;
using Superpower.Model;
using Superpower;
using Superpower.Parsers;

namespace JEM.Parse
{

    
    public class SExprTokenizer : Tokenizer<SExprToken>
    {
        protected TextParser<TextSpan> Symbol = Identifier.CStyle;
        

        readonly Dictionary<char, SExprToken> operators = new Dictionary<char, SExprToken>()
        {
            ['('] = SExprToken.Open,
            [')'] = SExprToken.Close
        };

        protected override IEnumerable<Result<SExprToken>> Tokenize(TextSpan span)
        {
            var next = SkipWhiteSpace(span);
            if (!next.HasValue)
            {
                yield break;
            }

            do
            {
                SExprToken token;

                var c = next.Value;
                if (operators.TryGetValue(c, out token))
                {
                    yield return Result.Value(token, next.Location, next.Remainder);
                    next = next.Remainder.ConsumeChar();
                }
                else
                {
                    var symbol = Symbol(next.Location);
                    next = symbol.Remainder.ConsumeChar();
                    yield return Result.Value(SExprToken.Symbol, symbol.Location, symbol.Remainder);
                }

                next = SkipWhiteSpace(next.Location);
            }
            while (next.HasValue);
        }
        
    }

}
