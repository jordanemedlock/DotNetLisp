using System;
using System.Collections.Generic;
using System.Text;

namespace JEM.Compile
{
    class Compiler<TInput,TOutput>
    {
        public virtual List<ITransformer<TInput, TOutput>> Transformers { get; set; }

        public virtual TOutput Compile(TInput input)
        {
            foreach (var transformer in Transformers)
            {
                if (transformer.MatchesPattern(input))
                {
                    return transformer.Transform(input);
                }
            }
            throw new ArgumentException("No pattern matches input");
        }
    }
}
