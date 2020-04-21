using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JEM.Model;

namespace JEM.Compile
{
    //public delegate CompilerResult<TInput, TOutput> Compiler<TInput, TOutput>(TInput input);

    public class Compiler<TInput, TOutput>
    {
        public string Name { get; set; }

        public Func<TInput, CompilerResult<TInput, TOutput>> Compile { get; }

        public Compiler(Func<TInput, CompilerResult<TInput, TOutput>> compile) {
            Compile = compile;
        }
        

        public Compiler(string name, Func<TInput, CompilerResult<TInput, TOutput>> compile)
        {
            Name = name;
            Compile = compile;
        }

        public Compiler(string name, Func<Compiler<TInput, TOutput>> func)
        {
            
            Compile = input =>
            {
                return func().Compile(input);
            };
            Name = name;
        }
        
        public Compiler<TInput, V> Bind<V>(Func<TOutput, Compiler<TInput, V>> func)
        {
            return new Compiler<TInput, V>($"{Name}.Bind()", input =>
            {
                var res = this.Compile(input);
                return res.Bind(val =>
                {
                    var innerCompiler = func(val);
                    var ret = innerCompiler.Compile(res.Remainder);
                    return ret;
                });
            });
        }

        public Compiler<TInput, V> Apply<V>(Compiler<TInput, V> other)
        {
            return new Compiler<TInput, V>($"{Name}.Apply()", input =>
            {
                var res = this.Compile(input);
                if (!res.HasValue)
                {
                    return new CompilerResult<TInput, V>(res.Error);
                } 
                else
                {
                    return other.Compile(res.Remainder);
                }
            });
        }

        public Compiler<TInput, V> Select<V>(Func<TOutput, V> func, string name = "")
        {
            return new Compiler<TInput, V>($"{Name}.Select({name})", input =>
            {
                var res = this.Compile(input);
                return res.Select(func);
            });
        }

        public Compiler<TInput, V> Return<V>(V value)
        {
            return new Compiler<TInput, V>($"{Name}.Return({value})", input =>
            {
                var res = this.Compile(input);
                return res.Return(value);
            });
        }

        public Compiler<TInput, TOutput> Where(Predicate<TOutput> pred, string name = "")
        {
            return new Compiler<TInput, TOutput>($"{Name}.Where({name})", input =>
            {
                var res = this.Compile(input);
                return res.Where(r => pred(r.Value), name);
            });
        }

        public Compiler<TInput, V> Is<V>() where V : class
        {
            return this.Where(x => x is V, $"{{0}} is not {typeof(V)})").Select(x => x as V);
        }

        public Compiler<TInput, TOutput> Or(Compiler<TInput, TOutput> other)
        {
            return new Compiler<TInput, TOutput>($"{Name}.Or({other.Name})", input =>
            {
                var results1 = this.Compile(input);
                if (!results1.HasValue)
                {
                    var results2 = other.Compile(input);
                    results2.HappyPath.Add(other.Name);
                    return results2;
                }
                else
                {
                    results1.HappyPath.Add(this.Name);
                    return results1;
                }
            });
        }

        public Compiler<TInput, TOutput> Or(Compiler<TInput, TOutput> other, string name)
        {
            return new Compiler<TInput, TOutput>($"{Name}.Or({other.Name}, {name})", input =>
            {
                var results1 = this.Compile(input);
                if (!results1.HasValue)
                {
                    var results2 = other.Compile(input);
                    results2.HappyPath.Add(other.Name);
                    return results2;
                }
                else
                {
                    results1.HappyPath.Add(this.Name);
                    return results1;
                }
            });
        }

        public static Compiler<TInput, TOutput> Or(params Compiler<TInput, TOutput>[] compilers)
        {
            return new Compiler<TInput, TOutput>($"Compiler.Or({ string.Join(", ", compilers.Select(x => x.Name)) })", input =>
            {
                CompilerResult<TInput, TOutput> result;
                int i = 0;
                do
                {
                    result = compilers[i++].Compile(input);
                }
                while (!result.HasValue && i < compilers.Length);
                return result;
            });
        }

        public static Compiler<TInput, TOutput> Or(string name, params Compiler<TInput, TOutput>[] compilers)
        {
            return new Compiler<TInput, TOutput>($"Compiler.Or({name}, { string.Join(", ", compilers.Select(x => x.Name)) })", input =>
            {
                CompilerResult<TInput, TOutput> result;
                List<CompilerResult<TInput, TOutput>> results = new List<CompilerResult<TInput, TOutput>>();
                int i = 0;
                do
                {
                    result = compilers[i++].Compile(input);
                    results.Add(result);
                }
                while (!result.HasValue && i < compilers.Length);
                result.HappyPath.Add($"{name}[{i - 1}]");
                if (i == compilers.Length && !result.HasValue) {
                    string errorMessages = "[\n";
                    for (int j=0; j < compilers.Length; j++) {
                        errorMessages += (results[j].HasValue ? results[j].Value.ToString() : results[j].Error) + "\n";
                    }
                    errorMessages += "]";
                    return new CompilerResult<TInput, TOutput>($"{input} doesn't match any compiler in {name} with errors: {errorMessages}");
                }
                return result;
            });
        }

        public static Compiler<TInput, TOutput> Return(TOutput output)
        {
            return new Compiler<TInput, TOutput>(input =>
            {
                return new CompilerResult<TInput, TOutput>(output, input);
            });
        }

        public static Compiler<TInput, TOutput> Error(string message)
        {
            return new Compiler<TInput, TOutput>(input =>
            {
                return new CompilerResult<TInput, TOutput>(message);
            });
        }
    }
}
